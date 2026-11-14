using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Swms.Api.Domain.Entities;
using Swms.Api.Features;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.FSRS;

namespace Swms.Tests;

public class DocumentCourseSyncTests
{
    private static WebApplicationFactory<Program> CreateIsolatedFactory(string? customDocRoot = null)
    {
        var dbName = $"SwmsDocSyncTest_{Guid.NewGuid():N}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
                if (!string.IsNullOrWhiteSpace(customDocRoot))
                {
                    builder.UseSetting("DocumentLibrary:RootPath", customDocRoot);
                }
            });
    }

    private static SwmsDbContext CreateIsolatedDbContext()
    {
        var options = new DbContextOptionsBuilder<SwmsDbContext>()
            .UseInMemoryDatabase($"SwmsDocSyncUnit_{Guid.NewGuid():N}")
            .Options;
        return new SwmsDbContext(options);
    }

    [Fact]
    public async Task SyncAndCatalog_LoadsAll7Courses_WithoutIgnoredMetaOrDuplicatePersonalFinance()
    {
        using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var courses = await client.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(courses);

        var expectedSlugs = new[]
        {
            "document-course-dotnet-react-roadmap",
            "document-course-fullstack-csharp-react-refresh",
            "document-course-ielts",
            "document-course-investing",
            "document-course-lifeskills",
            "document-course-personalfinance",
            "document-course-psychology"
        };

        var slugs = courses.Select(c => c.GetProperty("slug").GetString()!).ToList();
        foreach (var expected in expectedSlugs)
        {
            Assert.Contains(expected, slugs);
        }

        // No duplicate PersonalFinance card
        var pfCourses = courses.Where(c =>
            (c.GetProperty("title").GetString() ?? "").Contains("Tài Chính Cá Nhân", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.Single(pfCourses);

        // Catalog summary omits heavy contentMarkdown on nested lessons for performance
        foreach (var c in courses.Where(x => (x.GetProperty("slug").GetString() ?? "").StartsWith("document-course-")))
        {
            Assert.False(string.IsNullOrWhiteSpace(c.GetProperty("category").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(c.GetProperty("sourceFolder").GetString()));

            foreach (var ch in c.GetProperty("chapters").EnumerateArray())
            {
                foreach (var les in ch.GetProperty("lessons").EnumerateArray())
                {
                    if (les.TryGetProperty("contentMarkdown", out var mdProp))
                    {
                        Assert.Equal(JsonValueKind.Null, mdProp.ValueKind);
                    }

                    var srcPath = les.GetProperty("sourceRelativePath").GetString() ?? "";
                    Assert.DoesNotContain("HANDOFF", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("implementation_plan", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("REVIEW_REPORT", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("REVIEW_RULES", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("AGENTS.md", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("quality/", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("validation/", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("reference/", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("Chỉ nhìn doanh thu", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain(".py", srcPath, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain(".trx", srcPath, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

    [Fact]
    public async Task DotNetReactRoadmap_ExtractsStartDocx_AndDeduplicatesStageDocxFiles()
    {
        using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var courses = await client.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(courses);
        var dotnetCourse = courses.First(c => c.GetProperty("slug").GetString() == "document-course-dotnet-react-roadmap");

        var lessons = dotnetCourse.GetProperty("chapters")
            .EnumerateArray()
            .SelectMany(ch => ch.GetProperty("lessons").EnumerateArray())
            .ToList();

        // Expect 00_Bat_Dau.docx + README.md + stages 1..14 = 16 lessons (no duplicate 1_*.docx..14_*.docx)
        var startDocxLesson = lessons.FirstOrDefault(l =>
            string.Equals(l.GetProperty("sourceRelativePath").GetString(), "00_Bat_Dau.docx", StringComparison.OrdinalIgnoreCase));
        Assert.NotEqual(JsonValueKind.Undefined, startDocxLesson.ValueKind);

        var relPaths = lessons.Select(l => l.GetProperty("sourceRelativePath").GetString()!).ToList();
        Assert.Contains("1.md", relPaths);
        Assert.Contains("14.md", relPaths);
        Assert.DoesNotContain(relPaths, p => p.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) && !p.Equals("00_Bat_Dau.docx", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(16, lessons.Count);

        var startId = startDocxLesson.GetProperty("id").GetGuid();
        var startDetail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/lessons/{startId}");
        var contentMd = startDetail.GetProperty("contentMarkdown").GetString() ?? "";
        Assert.True(contentMd.Length > 100);
        Assert.Contains("| File |", contentMd);
        Assert.Contains("## Các chặng", contentMd);
        Assert.Contains("- ", contentMd);
        Assert.Equal("DotNet-React-Roadmap", startDetail.GetProperty("sourceFolder").GetString());
        Assert.Equal("00_Bat_Dau.docx", startDetail.GetProperty("sourceRelativePath").GetString());
    }

    [Fact]
    public async Task IeltsAudioAssetEndpoint_StreamsWavWithRangeSupport_AndBlocksPathTraversal()
    {
        using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var audioUrl = "/api/v1/document-assets/IELTS/Phase-1/audio/cycle11-listening-authored-practice.wav";
        var fullRes = await client.GetAsync(audioUrl);
        Assert.Equal(HttpStatusCode.OK, fullRes.StatusCode);
        Assert.Equal("audio/wav", fullRes.Content.Headers.ContentType?.MediaType);
        Assert.Contains("bytes", fullRes.Headers.AcceptRanges);
        var fullBytes = await fullRes.Content.ReadAsByteArrayAsync();
        Assert.True(fullBytes.Length > 1024);

        using var rangeReq = new HttpRequestMessage(HttpMethod.Get, audioUrl);
        rangeReq.Headers.Range = new RangeHeaderValue(0, 1023);
        var rangeRes = await client.SendAsync(rangeReq);
        Assert.Equal(HttpStatusCode.PartialContent, rangeRes.StatusCode);
        var partialBytes = await rangeRes.Content.ReadAsByteArrayAsync();
        Assert.Equal(1024, partialBytes.Length);

        // Traversal attempts must be rejected
        var encodedTraversal = await client.GetAsync("/api/v1/document-assets/IELTS/..%2F..%2Fbackend%2Fsrc%2FSwms.Api%2FProgram.cs");
        Assert.True(encodedTraversal.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.BadRequest);

        var pyBlocked = await client.GetAsync("/api/v1/document-assets/IELTS/check.py");
        Assert.True(pyBlocked.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.BadRequest);
    }

    [Fact]
    public void UpgradedQuizParser_ParsesBothNoDashFullStackAndInlinePromptInvestingWithAnswerKeyTables()
    {
        var now = DateTimeOffset.UtcNow;

        var fullStackQuizPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "FullStack-CSharp-React-Refresh", "assessments", "quizzes", "week-01-quiz.md");
        var fullStackAnsPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "FullStack-CSharp-React-Refresh", "assessments", "answer-keys", "week-01-answers.md");
        var fsItems = CourseWizardAndImportService.ParseQuizMarkdown(
            Guid.NewGuid(),
            File.ReadAllText(fullStackQuizPath),
            File.ReadAllText(fullStackAnsPath),
            now);

        Assert.Equal(6, fsItems.Count);
        Assert.All(fsItems, i => Assert.Equal("multiple_choice", i.ItemType));
        Assert.Equal("B", fsItems[0].ClozeSolution); // Q01-01 -> B
        Assert.Equal("C", fsItems[1].ClozeSolution); // Q01-02 -> C

        var investingQuizPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "Investing", "assessments", "quizzes", "week-01-quiz.md");
        var investingAnsPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "Investing", "assessments", "answer-keys", "week-01-answers.md");
        var invItems = CourseWizardAndImportService.ParseQuizMarkdown(
            Guid.NewGuid(),
            File.ReadAllText(investingQuizPath),
            File.ReadAllText(investingAnsPath),
            now);

        Assert.Equal(8, invItems.Count);
        Assert.Equal("A", invItems[0].ClozeSolution); // Q01-01 -> A
        var firstPrompt = JsonDocument.Parse(invItems[0].PromptAstJson).RootElement.GetProperty("question").GetString() ?? "";
        Assert.Contains("tính", firstPrompt, StringComparison.OrdinalIgnoreCase);

        var psychologyQuizPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "Psychology", "assessments", "quizzes", "week-01-quiz.md");
        var psychologyAnsPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "Psychology", "assessments", "answer-keys", "week-01-answers.md");
        var psyItems = CourseWizardAndImportService.ParseQuizMarkdown(
            Guid.NewGuid(),
            File.ReadAllText(psychologyQuizPath),
            File.ReadAllText(psychologyAnsPath),
            now);
        Assert.Equal(12, psyItems.Count);
        Assert.Equal("C", psyItems[0].ClozeSolution); // Q01-01 -> C

        var lifeSkillsQuizPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "LifeSkills", "assessments", "quizzes", "week-01-quiz.md");
        var lifeSkillsAnsPath = Path.Combine(DocumentCourseSyncService.DefaultDocumentRootPath, "LifeSkills", "assessments", "answer-keys", "week-01-answers.md");
        var lsItems = CourseWizardAndImportService.ParseQuizMarkdown(
            Guid.NewGuid(),
            File.ReadAllText(lifeSkillsQuizPath),
            File.ReadAllText(lifeSkillsAnsPath),
            now);
        Assert.True(lsItems.Count >= 6);
        Assert.Equal("B", lsItems[0].ClozeSolution);
    }

    [Fact]
    public async Task FlashcardMatchingAndCourseFiltering_LinksWeek3CardsToWeek3Lesson_AndFiltersDueByCourseId()
    {
        using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var courses = await client.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(courses);

        var investing = courses.First(c => c.GetProperty("slug").GetString() == "document-course-investing");
        var investingId = investing.GetProperty("id").GetGuid();

        var lessons = investing.GetProperty("chapters")
            .EnumerateArray()
            .SelectMany(ch => ch.GetProperty("lessons").EnumerateArray())
            .ToList();

        var week3Lesson = lessons.First(l =>
            (l.GetProperty("sourceRelativePath").GetString() ?? "").Contains("week-03-bao-cao-tai-chinh.md", StringComparison.OrdinalIgnoreCase));
        var week3Id = week3Lesson.GetProperty("id").GetGuid();

        // Flashcards linked specifically to Week 3 lesson (not dumped onto Week 1 fallback)
        var week3Cards = await client.GetFromJsonAsync<List<JsonElement>>($"/api/v1/flashcards/due?lessonId={week3Id}&includeAll=true");
        Assert.NotNull(week3Cards);
        Assert.Equal(8, week3Cards.Count);
        Assert.All(week3Cards, fc => Assert.Equal(week3Id, fc.GetProperty("lessonId").GetGuid()));

        // Course-wide flashcard filtering returns Investing cards only
        var investingCards = await client.GetFromJsonAsync<List<JsonElement>>($"/api/v1/flashcards/due?courseId={investingId}&includeAll=true");
        Assert.NotNull(investingCards);
        Assert.True(investingCards.Count >= 100);
        var investingLessonIds = lessons.Select(l => l.GetProperty("id").GetGuid()).ToHashSet();
        Assert.All(investingCards, fc => Assert.Contains(fc.GetProperty("lessonId").GetGuid(), investingLessonIds));

        // Practice quizzes endpoint supports ?courseId= filter and emits courseId
        var investingQuizzes = await client.GetFromJsonAsync<List<JsonElement>>($"/api/v1/practice-quizzes?courseId={investingId}");
        Assert.NotNull(investingQuizzes);
        Assert.Equal(18, investingQuizzes.Count);
        Assert.All(investingQuizzes, q => Assert.Equal(investingId, q.GetProperty("courseId").GetGuid()));
    }

    [Fact]
    public async Task LessonEnrichmentAndRelativePathResolution_WorksAcrossIeltsTranscriptLinks()
    {
        using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var courses = await client.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(courses);
        var ielts = courses.First(c => c.GetProperty("slug").GetString() == "document-course-ielts");
        var ieltsId = ielts.GetProperty("id").GetGuid();

        var ieltsLessons = ielts.GetProperty("chapters")
            .EnumerateArray()
            .SelectMany(ch => ch.GetProperty("lessons").EnumerateArray())
            .ToList();

        var cycle11Listening = ieltsLessons.First(l =>
            string.Equals(l.GetProperty("sourceRelativePath").GetString(), "Phase-1/listening/cycle11-listening.md", StringComparison.OrdinalIgnoreCase));
        var listeningId = cycle11Listening.GetProperty("id").GetGuid();

        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/lessons/{listeningId}");
        Assert.Equal("IELTS", detail.GetProperty("sourceFolder").GetString());
        Assert.Equal("Phase-1/listening/cycle11-listening.md", detail.GetProperty("sourceRelativePath").GetString());
        Assert.NotEqual(JsonValueKind.Null, detail.GetProperty("adjacentPrevLessonId").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, detail.GetProperty("adjacentNextLessonId").ValueKind);

        var resolveResp = await client.GetFromJsonAsync<JsonElement>(
            $"/api/v1/lessons/by-path?courseId={ieltsId}&fromLessonId={listeningId}&relativePath=../transcripts/cycle11-listening-authored-practice.md");
        Assert.True(resolveResp.GetProperty("found").GetBoolean());
        Assert.Equal("Phase-1/transcripts/cycle11-listening-authored-practice.md", resolveResp.GetProperty("sourceRelativePath").GetString());
        Assert.NotEqual(Guid.Empty, resolveResp.GetProperty("lessonId").GetGuid());
    }

    [Fact]
    public async Task SyncIsIdempotent_PreservesUserProgressAndMigratesLegacyPersonalFinanceWithoutDuplication()
    {
        using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var coursesBefore = await client.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(coursesBefore);
        var pfCourse = coursesBefore.First(c => c.GetProperty("slug").GetString() == "document-course-personalfinance");
        var pfId = pfCourse.GetProperty("id").GetGuid();
        var pfLessonsBefore = pfCourse.GetProperty("chapters").EnumerateArray().SelectMany(ch => ch.GetProperty("lessons").EnumerateArray()).ToList();
        var pfLesson0Id = pfLessonsBefore.First().GetProperty("id").GetGuid();

        // Create user progress & annotation before re-sync
        var completeResp = await client.PostAsync($"/api/v1/lessons/{pfLesson0Id}/complete", null);
        completeResp.EnsureSuccessStatusCode();

        // Trigger explicit re-sync
        var syncResp = await client.PostAsync("/api/v1/courses/sync-from-documents", null);
        syncResp.EnsureSuccessStatusCode();
        var syncJson = await syncResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(7, syncJson.GetProperty("syncedCourses").GetInt32());
        Assert.Equal(0, syncJson.GetProperty("createdCourses").GetInt32());
        Assert.Equal(0, syncJson.GetProperty("createdLessons").GetInt32());
        Assert.Equal(0, syncJson.GetProperty("createdQuizzes").GetInt32());
        Assert.Equal(0, syncJson.GetProperty("createdFlashcards").GetInt32());

        // Verify progress preserved
        var progressResp = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{pfId}/progress");
        var completedIds = progressResp.GetProperty("completedLessonIds").EnumerateArray().Select(x => x.GetGuid()).ToList();
        Assert.Contains(pfLesson0Id, completedIds);

        // Unit check: legacy seeded PersonalFinance course with old slug upgrades cleanly in-place preserving progress
        using var db = CreateIsolatedDbContext();
        await SeedDataInitializer.SeedAsync(db, Path.Combine(Path.GetTempPath(), "NonExistentDocRoot_" + Guid.NewGuid().ToString("N")));

        var legacyPf = await db.Courses.FirstOrDefaultAsync(c => c.Title.Contains("Tài Chính Cá Nhân"));
        if (legacyPf != null)
        {
            var oldLesson = await db.Lessons
                .Join(db.Chapters.Where(ch => ch.CourseId == legacyPf.Id), l => l.ChapterId, ch => ch.Id, (l, _) => l)
                .OrderBy(l => l.SortOrder)
                .FirstAsync();

            db.UserLessonProgresses.Add(new UserLessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = SeedDataInitializer.DefaultUserId,
                LessonId = oldLesson.Id,
                CompletedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            await DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, DocumentCourseSyncService.DefaultDocumentRootPath, useSnapshotCache: false);

            var allPf = await db.Courses.Where(c => c.Title.Contains("Tài Chính Cá Nhân")).ToListAsync();
            Assert.Single(allPf);
            Assert.Equal("document-course-personalfinance", allPf[0].Slug);
            Assert.Equal(legacyPf.Id, allPf[0].Id);
            Assert.True(await db.UserLessonProgresses.AnyAsync(p => p.UserId == SeedDataInitializer.DefaultUserId && p.LessonId == oldLesson.Id));
        }
    }

    [Fact]
    public async Task RealSqliteDatabase_SyncsAll7CoursesCleanly_AndRemainsIdempotent()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        var options = new DbContextOptionsBuilder<SwmsDbContext>()
            .UseSqlite(conn)
            .Options;

        await using var db = new SwmsDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await SeedDataInitializer.SeedAsync(db, DocumentCourseSyncService.DefaultDocumentRootPath);

        var syncedCourses = await db.Courses
            .Where(c => c.Slug.StartsWith("document-course-"))
            .ToListAsync();
        Assert.Equal(7, syncedCourses.Count);

        var totalLessonsBefore = await db.Lessons.CountAsync();
        var totalQuizzesBefore = await db.PracticeQuizzes.CountAsync();
        var totalCardsBefore = await db.Flashcards.CountAsync();

        Assert.True(totalLessonsBefore >= 250);
        Assert.True(totalQuizzesBefore >= 100);
        Assert.True(totalCardsBefore >= 800);

        var secondReport = await DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, DocumentCourseSyncService.DefaultDocumentRootPath, useSnapshotCache: false);
        Assert.Equal(7, secondReport.SyncedCourses);
        Assert.Equal(0, secondReport.CreatedCourses);
        Assert.Equal(0, secondReport.CreatedLessons);
        Assert.Equal(0, secondReport.CreatedQuizzes);
        Assert.Equal(0, secondReport.CreatedFlashcards);
        Assert.Equal(totalLessonsBefore, await db.Lessons.CountAsync());
        Assert.Equal(totalQuizzesBefore, await db.PracticeQuizzes.CountAsync());
        Assert.Equal(totalCardsBefore, await db.Flashcards.CountAsync());
    }

    private static WebApplicationFactory<Program> CreateRealSqliteFactory(SqliteConnection openConnection)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<SwmsDbContext>) ||
                                    (d.ServiceType.IsGenericType &&
                                     d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal)))
                        .ToList();
                    foreach (var d in descriptors)
                    {
                        services.Remove(d);
                    }
                    services.AddDbContext<SwmsDbContext>(options => options.UseSqlite(openConnection));
                });
            });
    }

    [Fact]
    public async Task RealSqliteHttpEndpoints_RegisteredUserAndDefaultUser_FixesAll4E2EBugsWithIsolation()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        await using var factory = CreateRealSqliteFactory(conn);
        using var defaultClient = factory.CreateClient();

        // 1. Catalog sort order for DefaultUser: all 7 document-course-* in curriculum order at the top
        var defaultCourses = await defaultClient.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(defaultCourses);
        var defaultDocSlugs = defaultCourses
            .Select(c => c.GetProperty("slug").GetString() ?? "")
            .Take(7)
            .ToArray();
        Assert.Equal(new[]
        {
            "document-course-dotnet-react-roadmap",
            "document-course-fullstack-csharp-react-refresh",
            "document-course-ielts",
            "document-course-investing",
            "document-course-personalfinance",
            "document-course-lifeskills",
            "document-course-psychology"
        }, defaultDocSlugs);

        var investingCourse = defaultCourses.First(c => c.GetProperty("slug").GetString() == "document-course-investing");
        var investingId = investingCourse.GetProperty("id").GetGuid();
        var investingCardCountBefore = investingCourse.GetProperty("flashcardCount").GetInt32();
        var investingLessons = investingCourse.GetProperty("chapters")
            .EnumerateArray()
            .SelectMany(ch => ch.GetProperty("lessons").EnumerateArray())
            .ToList();
        var investingWeek1Id = investingLessons.First(l => (l.GetProperty("sourceRelativePath").GetString() ?? "").Contains("week-01")).GetProperty("id").GetGuid();
        var investingWeek3Id = investingLessons.First(l => (l.GetProperty("sourceRelativePath").GetString() ?? "").Contains("week-03")).GetProperty("id").GetGuid();

        // 2. Register a brand new user -> authenticates as registered user (userId != DefaultUserId)
        var regEmail = $"e2e.{Guid.NewGuid():N}@studxy.vn";
        var regResp = await defaultClient.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(regEmail, "Studxy@2026", "E2E Verifier"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = regJson.GetProperty("accessToken").GetString();
        var regUserId = regJson.GetProperty("user").GetProperty("id").GetGuid();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        using var regClient = factory.CreateClient();
        regClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // 3. Bug 1 Fix: GET /api/v1/lessons/{id} works cleanly on real SQLite (no DateTimeOffset ORDER BY 500!)
        var defaultLessonResp = await defaultClient.GetAsync($"/api/v1/lessons/{investingWeek1Id}");
        Assert.Equal(HttpStatusCode.OK, defaultLessonResp.StatusCode);

        var regLessonResp = await regClient.GetAsync($"/api/v1/lessons/{investingWeek1Id}");
        Assert.Equal(HttpStatusCode.OK, regLessonResp.StatusCode);
        var regLessonJson = await regLessonResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(regLessonJson.TryGetProperty("linkedQuizId", out var linkedQuizProp));
        Assert.Equal(JsonValueKind.String, linkedQuizProp.ValueKind);

        var regWeek3Resp = await regClient.GetAsync($"/api/v1/lessons/{investingWeek3Id}");
        Assert.Equal(HttpStatusCode.OK, regWeek3Resp.StatusCode);
        var regWeek3Json = await regWeek3Resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(8, regWeek3Json.GetProperty("linkedFlashcardsCount").GetInt32());

        // 4. Bug 2 Fix: Registered user sees document-course-* quizzes in GET /practice-quizzes (without ?courseId=)
        //    and /workspace/dashboard, can GET and POST submit answers (no 403), while DefaultQuizId stays 403!
        var regQuizzes = await regClient.GetFromJsonAsync<List<JsonElement>>("/api/v1/practice-quizzes");
        Assert.NotNull(regQuizzes);
        Assert.True(regQuizzes.Count >= 100);
        Assert.All(regQuizzes, q => Assert.Equal(JsonValueKind.String, q.GetProperty("courseId").ValueKind));
        Assert.DoesNotContain(regQuizzes, q => q.GetProperty("id").GetGuid() == SeedDataInitializer.DefaultQuizId);

        var regDashboard = await regClient.GetFromJsonAsync<JsonElement>("/api/v1/workspace/dashboard");
        Assert.True(regDashboard.GetProperty("quizzes").GetArrayLength() >= 100);

        var regForbiddenDefaultQuizResp = await regClient.GetAsync($"/api/v1/practice-quizzes/{SeedDataInitializer.DefaultQuizId}");
        Assert.Equal(HttpStatusCode.Forbidden, regForbiddenDefaultQuizResp.StatusCode);

        var investingQuizzes = await regClient.GetFromJsonAsync<List<JsonElement>>($"/api/v1/practice-quizzes?courseId={investingId}");
        Assert.NotNull(investingQuizzes);
        Assert.NotEmpty(investingQuizzes);
        var quizId = investingQuizzes[0].GetProperty("id").GetGuid();

        var quizDetailResp = await regClient.GetAsync($"/api/v1/practice-quizzes/{quizId}");
        Assert.Equal(HttpStatusCode.OK, quizDetailResp.StatusCode);
        var quizDetail = await quizDetailResp.Content.ReadFromJsonAsync<JsonElement>();
        var firstItem = quizDetail.GetProperty("items")[0];
        var firstItemId = firstItem.GetProperty("id").GetGuid();
        var expectedKey = firstItem.GetProperty("clozeSolution").GetString()!;

        // Submit right option
        var submitRightResp = await regClient.PostAsJsonAsync(
            $"/api/v1/practice-quizzes/{quizId}/items/{firstItemId}/submit",
            new SubmitQuizItemRequest(expectedKey, null, false));
        Assert.Equal(HttpStatusCode.OK, submitRightResp.StatusCode);
        var submitRightJson = await submitRightResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(submitRightJson.GetProperty("isCorrect").GetBoolean());

        // Submit wrong option whose text contains the expected letter (regression check for multiple_choice false positives)
        var wrongOption = expectedKey.Equals("A", StringComparison.OrdinalIgnoreCase)
            ? "B. RAM & API"
            : $"Z. Option {expectedKey} wrong";
        var submitWrongResp = await regClient.PostAsJsonAsync(
            $"/api/v1/practice-quizzes/{quizId}/items/{firstItemId}/submit",
            new SubmitQuizItemRequest(wrongOption, null, false));
        Assert.Equal(HttpStatusCode.OK, submitWrongResp.StatusCode);
        var submitWrongJson = await submitWrongResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(submitWrongJson.GetProperty("isCorrect").GetBoolean());

        // DefaultQuizId submit must STILL be 403 Forbidden for registered user
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var defaultQuizItem = await db.PracticeQuizItems.FirstAsync(i => i.QuizId == SeedDataInitializer.DefaultQuizId);
            var forbiddenSubmit = await regClient.PostAsJsonAsync(
                $"/api/v1/practice-quizzes/{SeedDataInitializer.DefaultQuizId}/items/{defaultQuizItem.Id}/submit",
                new SubmitQuizItemRequest("A", null, false));
            Assert.Equal(HttpStatusCode.Forbidden, forbiddenSubmit.StatusCode);
        }

        // 5. Bug 3 Fix: Registered user lazily clones public document-course-* flashcards with isolated FSRS state
        //    and without double-counting course.flashcardCount or lesson.linkedFlashcardsCount
        var regInvestingCards = await regClient.GetFromJsonAsync<List<JsonElement>>(
            $"/api/v1/flashcards/due?courseId={investingId}&includeAll=true");
        Assert.NotNull(regInvestingCards);
        Assert.Equal(investingCardCountBefore, regInvestingCards.Count);

        var regWeek3Cards = await regClient.GetFromJsonAsync<List<JsonElement>>(
            $"/api/v1/flashcards/due?lessonId={investingWeek3Id}&includeAll=true");
        Assert.NotNull(regWeek3Cards);
        Assert.Equal(8, regWeek3Cards.Count);

        var clonedCardId = regWeek3Cards[0].GetProperty("id").GetGuid();
        var reviewResp = await regClient.PostAsJsonAsync("/api/v1/flashcards/sync-reviews",
            new SyncReviewsBatchRequest(new List<FsrsReviewEventInput>
            {
                new(Guid.NewGuid(), clonedCardId, 3, DateTimeOffset.UtcNow, 1200)
            }));
        Assert.Equal(HttpStatusCode.OK, reviewResp.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var userCard = await db.Flashcards.AsNoTracking().FirstAsync(f => f.Id == clonedCardId);
            Assert.Equal(regUserId, userCard.UserId);
            Assert.Equal(1, userCard.Reps);

            var defaultCardsForLesson = await db.Flashcards
                .AsNoTracking()
                .Where(f => f.LessonId == investingWeek3Id && f.UserId == SeedDataInitializer.DefaultUserId)
                .ToListAsync();
            Assert.Equal(8, defaultCardsForLesson.Count);
            Assert.All(defaultCardsForLesson, dc => Assert.Equal(0, dc.Reps));
        }

        var regInvestingAfter = await regClient.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{investingId}");
        Assert.Equal(investingCardCountBefore, regInvestingAfter.GetProperty("flashcardCount").GetInt32());

        var regWeek3After = await regClient.GetFromJsonAsync<JsonElement>($"/api/v1/lessons/{investingWeek3Id}");
        Assert.Equal(8, regWeek3After.GetProperty("linkedFlashcardsCount").GetInt32());

        // Also test ?tagId=<DefaultUserId course tagId> remaps cleanly for registered user
        var defaultWeek3Cards = await defaultClient.GetFromJsonAsync<List<JsonElement>>(
            $"/api/v1/flashcards/due?lessonId={investingWeek3Id}&includeAll=true");
        Assert.NotNull(defaultWeek3Cards);
        var defaultCourseTagId = defaultWeek3Cards[0].GetProperty("tags")[0].GetProperty("id").GetGuid();
        var defaultByTag = await defaultClient.GetFromJsonAsync<List<JsonElement>>(
            $"/api/v1/flashcards/due?tagId={defaultCourseTagId}&includeAll=true");
        Assert.NotNull(defaultByTag);
        Assert.NotEmpty(defaultByTag);

        var regByDefaultTag = await regClient.GetFromJsonAsync<List<JsonElement>>(
            $"/api/v1/flashcards/due?tagId={defaultCourseTagId}&includeAll=true");
        Assert.NotNull(regByDefaultTag);
        Assert.Equal(defaultByTag.Count, regByDefaultTag.Count);

        // 6. Bug 4a Fix: Registered user's own created/imported course sorts FIRST before document-course-*
        var templateZipBytes = await regClient.GetByteArrayAsync("/api/v1/courses/template-zip");
        using var zipContent = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(templateZipBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        zipContent.Add(fileContent, "file", "template.zip");

        var importResp = await regClient.PostAsync("/api/v1/courses/import-zip", zipContent);
        Assert.Equal(HttpStatusCode.OK, importResp.StatusCode);
        var importedCourseJson = await importResp.Content.ReadFromJsonAsync<JsonElement>();
        var importedCourseId = importedCourseJson.GetProperty("id").GetGuid();

        var regCoursesAfterImport = await regClient.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(regCoursesAfterImport);
        Assert.Equal(importedCourseId, regCoursesAfterImport[0].GetProperty("id").GetGuid());
        Assert.Equal("document-course-dotnet-react-roadmap", regCoursesAfterImport[1].GetProperty("slug").GetString());
    }

    [Fact]
    public async Task RealSqlite_ReconcileDuplicateLegacyPersonalFinanceLessons_MergesOrphansAndPreservesProgress()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        var options = new DbContextOptionsBuilder<SwmsDbContext>()
            .UseSqlite(conn)
            .Options;

        await using var db = new SwmsDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await SeedDataInitializer.SeedAsync(db, DocumentCourseSyncService.DefaultDocumentRootPath);

        var pfCourse = await db.Courses.FirstAsync(c => c.Slug == "document-course-personalfinance");
        var now = DateTimeOffset.UtcNow;

        // Simulate legacy orphan duplicate chapter + 2 lessons lacking sourceRelativePath ("Tuần 01", "Tuần 12")
        var orphanChapter = new Chapter
        {
            Id = Guid.NewGuid(),
            CourseId = pfCourse.Id,
            Title = "Chương di sản cũ",
            SortOrder = 99
        };
        db.Chapters.Add(orphanChapter);

        var orphanWeek1 = new Lesson
        {
            Id = Guid.NewGuid(),
            ChapterId = orphanChapter.Id,
            Title = "Tuần 01: Nhận thức về Tiền bạc & Tâm lý Tài chính",
            Slug = "tuan-01-legacy",
            SortOrder = 1,
            ContentAstJson = "{\"type\":\"doc\",\"content\":[]}",
            ContentMarkdown = "# Old Week 1",
            CurrentVersion = 1,
            UpdatedAt = now.AddDays(-5)
        };
        var orphanWeek12 = new Lesson
        {
            Id = Guid.NewGuid(),
            ChapterId = orphanChapter.Id,
            Title = "Tuần 12: Tổng kết & Kế hoạch 5 năm",
            Slug = "tuan-12-legacy",
            SortOrder = 2,
            ContentAstJson = "{\"type\":\"doc\",\"content\":[]}",
            ContentMarkdown = "# Old Week 12",
            CurrentVersion = 1,
            UpdatedAt = now.AddDays(-5)
        };
        db.Lessons.AddRange(orphanWeek1, orphanWeek12);

        // Attach learner data onto the orphan Week 12 lesson that MUST survive deduplication
        var regUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"pf-merge-{Guid.NewGuid():N}@studxy.vn",
            PasswordHash = "hash",
            FullName = "PF Merge Tester",
            Role = "student",
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Users.Add(regUser);

        db.UserLessonProgresses.Add(new UserLessonProgress
        {
            Id = Guid.NewGuid(),
            UserId = regUser.Id,
            LessonId = orphanWeek12.Id,
            IsCompleted = true,
            CompletedAt = now.AddDays(-1)
        });

        var orphanAnn = new Annotation
        {
            Id = Guid.NewGuid(),
            UserId = regUser.Id,
            LessonId = orphanWeek12.Id,
            BlockId = Guid.NewGuid(),
            ColorKey = "yellow",
            ExactQuote = "Kế hoạch 5 năm",
            OriginalExactQuote = "Kế hoạch 5 năm",
            StartOffset = 0,
            EndOffset = 14,
            Status = "anchored",
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Annotations.Add(orphanAnn);

        var orphanNotebook = new Notebook
        {
            Id = Guid.NewGuid(),
            UserId = regUser.Id,
            PrimarySourceLessonId = orphanWeek12.Id,
            Title = "Ghi chú Tuần 12 di sản",
            ContentAstJson = "{}",
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Notebooks.Add(orphanNotebook);
        db.NotebookLinks.Add(new NotebookLink
        {
            Id = Guid.NewGuid(),
            NotebookId = orphanNotebook.Id,
            RefLessonId = orphanWeek12.Id,
            CreatedAt = now
        });

        var reviewedCard = new Flashcard
        {
            Id = Guid.NewGuid(),
            UserId = regUser.Id,
            LessonId = orphanWeek12.Id,
            CardType = "cloze",
            FrontAstJson = "{\"text\":\"Mục tiêu 5 năm\"}",
            BackAstJson = "{\"text\":\"Tài chính bền vững\"}",
            Stability = 2.5,
            Difficulty = 5.0,
            Reps = 1,
            State = 2,
            DueAt = now.AddDays(2),
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Flashcards.Add(reviewedCard);
        db.FlashcardReviews.Add(new FlashcardReview
        {
            Id = Guid.NewGuid(),
            ReviewEventId = Guid.NewGuid(),
            FlashcardId = reviewedCard.Id,
            UserId = regUser.Id,
            Rating = 3,
            ReviewDurationMs = 900,
            ScheduledDays = 2.0,
            ElapsedDays = 0.0,
            LastStability = 0.0,
            NewStability = 2.5,
            LastDifficulty = 5.0,
            NewDifficulty = 5.0,
            ReviewedAt = now
        });
        await db.SaveChangesAsync();

        // Act: re-sync from documents (force full sync bypassing snapshot cache)
        await DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, DocumentCourseSyncService.DefaultDocumentRootPath, useSnapshotCache: false);

        // Assert: only 14 canonical PersonalFinance lessons remain (no orphan duplicates, empty orphan chapter deleted)
        var pfChapters = await db.Chapters.Where(ch => ch.CourseId == pfCourse.Id).ToListAsync();
        Assert.DoesNotContain(pfChapters, ch => ch.Id == orphanChapter.Id);

        var pfChapterIds = pfChapters.Select(ch => ch.Id).ToHashSet();
        var pfLessons = await db.Lessons.Where(l => pfChapterIds.Contains(l.ChapterId)).ToListAsync();
        Assert.Equal(14, pfLessons.Count);
        Assert.All(pfLessons, l =>
            Assert.False(string.IsNullOrWhiteSpace(CourseGenerationService.ExtractLessonSourceMetadata(l.ContentAstJson).SourceRelativePath)));

        var canonicalWeek12 = pfLessons.First(l =>
            CourseGenerationService.ExtractLessonSourceMetadata(l.ContentAstJson).SourceRelativePath!.Contains("week-12", StringComparison.OrdinalIgnoreCase));

        // Learner data preserved and migrated onto canonical Week 12 lesson
        Assert.True(await db.UserLessonProgresses.AnyAsync(p => p.UserId == regUser.Id && p.LessonId == canonicalWeek12.Id && p.IsCompleted));
        var migratedAnn = await db.Annotations.FirstAsync(a => a.Id == orphanAnn.Id);
        Assert.Equal(canonicalWeek12.Id, migratedAnn.LessonId);
        var migratedNb = await db.Notebooks.FirstAsync(n => n.Id == orphanNotebook.Id);
        Assert.Equal(canonicalWeek12.Id, migratedNb.PrimarySourceLessonId);
        var migratedLink = await db.NotebookLinks.FirstAsync(nl => nl.NotebookId == orphanNotebook.Id);
        Assert.Equal(canonicalWeek12.Id, migratedLink.RefLessonId);
        var migratedCard = await db.Flashcards.FirstAsync(f => f.Id == reviewedCard.Id);
        Assert.Equal(canonicalWeek12.Id, migratedCard.LessonId);
        Assert.True(await db.FlashcardReviews.AnyAsync(r => r.FlashcardId == reviewedCard.Id));
    }
}
