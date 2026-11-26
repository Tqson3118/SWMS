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
using Xunit;

namespace Swms.Tests;

public class CourseLifecycleUpgradeTests
{
    private static WebApplicationFactory<Program> CreateRealSqliteFactory(SqliteConnection connection)
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
                    services.AddDbContext<SwmsDbContext>(options => options.UseSqlite(connection));
                });
            });
    }

    private static async Task<(WebApplicationFactory<Program> Factory, HttpClient Client, SqliteConnection Connection)> CreateTestHostAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var factory = CreateRealSqliteFactory(connection);
        var client = factory.CreateClient();

        // Warmup and seed full catalog deterministically
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            await DocumentCourseSyncService.SyncStudxyBootcampCourseAsync(db);
            await DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, DocumentCourseSyncService.DefaultDocumentRootPath);
            await CuratedCourseMetadataCatalog.ApplyDefaultsToCoursesAsync(db);
        }

        return (factory, client, connection);
    }

    [Fact]
    public async Task GetCourses_ExcludesAllRetiredCourses_AndReturns14CuratedCourses_WhenFullCatalogSynced()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var courses = await client.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
            Assert.NotNull(courses);

            var expectedCuratedSlugs = new[]
            {
                "doc-course-studxy-fullstack-bootcamp",
                "document-course-ielts",
                "document-course-investing",
                "document-course-personalfinance",
                "document-course-lifeskills",
                "document-course-psychology",
                "document-course-ai-workflows",
                "document-course-data-sql",
                "document-course-cybersecurity",
                "document-course-deep-learning-systems",
                "he-dieu-hanh-bo-nho-ao",
                "tieng-anh-giao-tiep-thuc-chien-a2-b1",
                "tieng-nhat-so-cap-n5",
                "tieng-trung-so-cap-hsk1"
            };
            Assert.Equal(14, expectedCuratedSlugs.Length);

            var returnedSlugs = courses.Select(c => c.GetProperty("slug").GetString()!).ToList();

            foreach (var curated in expectedCuratedSlugs)
            {
                Assert.Contains(curated, returnedSlugs);
            }

            // Excludes all 5 retired slugs
            var retiredSlugs = new[]
            {
                "document-course-dotnet-react-roadmap",
                "document-course-fullstack-csharp-react-refresh",
                "lap-trinh-javascript-tu-duy-giai-quyet-van-de",
                "dau-tu-quan-tri-danh-muc-thuc-chien",
                "tam-ly-hoc-ung-dung-khoa-hoc-tri-nho"
            };
            foreach (var retired in retiredSlugs)
            {
                Assert.DoesNotContain(retired, returnedSlugs);
            }

            // Verify metadata fields for curated courses
            foreach (var c in courses.Where(x => expectedCuratedSlugs.Contains(x.GetProperty("slug").GetString())))
            {
                Assert.False(string.IsNullOrWhiteSpace(c.GetProperty("category").GetString()), "Category should not be empty");
                var level = c.GetProperty("level").GetString();
                Assert.True(level is "foundation" or "intermediate" or "advanced", $"Level '{level}' not recognized");

                var outcomes = c.GetProperty("learningOutcomes");
                Assert.Equal(JsonValueKind.Array, outcomes.ValueKind);
                Assert.True(outcomes.GetArrayLength() > 0, "learningOutcomes should have items");

                var prereqs = c.GetProperty("prerequisites");
                Assert.Equal(JsonValueKind.Array, prereqs.ValueKind);
                Assert.True(prereqs.GetArrayLength() > 0, "prerequisites should have items");

                Assert.False(string.IsNullOrWhiteSpace(c.GetProperty("labLabel").GetString()), "labLabel should not be empty");
                Assert.True(c.TryGetProperty("hasAudio", out var audioProp) && audioProp.ValueKind is JsonValueKind.True or JsonValueKind.False);
                Assert.True(c.GetProperty("isSystemCourse").GetBoolean(), "isSystemCourse must be true for curated system course");
                Assert.False(c.GetProperty("canDelete").GetBoolean(), "canDelete must be false for protected system course");
                Assert.True(c.TryGetProperty("isPinned", out _));
                Assert.True(c.TryGetProperty("isArchived", out _));
                Assert.True(c.GetProperty("dailyLessonGoal").GetInt32() >= 1);
            }
        }
    }

    [Fact]
    public async Task GetCourseById_RetiredCourseIds_Return404WithRedirectMetadata_OnFreshAndLegacyDb()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            // Fresh DB: CodingCourseId, InvestingCourseId, PsychologyCourseId are retired
            var testCases = new (Guid Id, string ExpectedRedirectSlug)[]
            {
                (SeedDataInitializer.CodingCourseId, "doc-course-studxy-fullstack-bootcamp"),
                (SeedDataInitializer.InvestingCourseId, "document-course-investing"),
                (SeedDataInitializer.PsychologyCourseId, "document-course-psychology")
            };

            foreach (var (id, expectedRedirect) in testCases)
            {
                var resp = await client.GetAsync($"/api/v1/courses/{id}");
                Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

                var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
                Assert.True(json.GetProperty("retired").GetBoolean());
                Assert.Equal(expectedRedirect, json.GetProperty("redirectSlug").GetString());
                Assert.True(json.TryGetProperty("redirectCourseId", out _));
            }

            // Legacy DB simulation: course row exists in DB with retired slug
            var legacyCourseId = Guid.NewGuid();
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
                db.Courses.Add(new Course
                {
                    Id = legacyCourseId,
                    InstructorId = SeedDataInitializer.DefaultUserId,
                    Title = "Legacy Retired JS Course",
                    Slug = "lap-trinh-javascript-tu-duy-giai-quyet-van-de",
                    Visibility = "public",
                    IsPublished = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var legacyResp = await client.GetAsync($"/api/v1/courses/{legacyCourseId}");
            Assert.Equal(HttpStatusCode.NotFound, legacyResp.StatusCode);
            var legacyJson = await legacyResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(legacyJson.GetProperty("retired").GetBoolean());
            Assert.Equal("doc-course-studxy-fullstack-bootcamp", legacyJson.GetProperty("redirectSlug").GetString());
        }
    }

    [Fact]
    public async Task CoursePreferences_PinArchiveDailyGoalTargetDate_PersistAcrossRequests()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var courseId = SeedDataInitializer.AcademicCourseId;
            var targetDate = DateTimeOffset.UtcNow.AddMonths(2);

            var putPrefReq = new UpdateCoursePreferencesRequest(
                IsPinned: true,
                IsArchived: true,
                DailyLessonGoal: 5,
                TargetCompletionDate: targetDate,
                ClearTargetDate: false);

            var putResp = await client.PutAsJsonAsync($"/api/v1/courses/{courseId}/preferences", putPrefReq);
            Assert.Equal(HttpStatusCode.OK, putResp.StatusCode);
            var putJson = await putResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(putJson.GetProperty("isPinned").GetBoolean());
            Assert.True(putJson.GetProperty("isArchived").GetBoolean());
            Assert.Equal(5, putJson.GetProperty("dailyLessonGoal").GetInt32());

            // Verify persisted in GET /courses/{id}
            var getResp = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
            Assert.True(getResp.GetProperty("isPinned").GetBoolean());
            Assert.True(getResp.GetProperty("isArchived").GetBoolean());
            Assert.Equal(5, getResp.GetProperty("dailyLessonGoal").GetInt32());
            Assert.True(getResp.TryGetProperty("targetCompletionDate", out var targetProp) && targetProp.ValueKind != JsonValueKind.Null);

            // Test clearTargetDate: true
            var clearReq = new UpdateCoursePreferencesRequest(
                IsPinned: false,
                ClearTargetDate: true);
            var clearResp = await client.PutAsJsonAsync($"/api/v1/courses/{courseId}/preferences", clearReq);
            Assert.Equal(HttpStatusCode.OK, clearResp.StatusCode);

            var getAfterClear = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
            Assert.False(getAfterClear.GetProperty("isPinned").GetBoolean());
            Assert.Equal(JsonValueKind.Null, getAfterClear.GetProperty("targetCompletionDate").ValueKind);
        }
    }

    [Fact]
    public async Task CourseLifecycle_Clone_UpdateMetadata_And_DeletePersonalVsProtectedSystemCourse()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var systemId = SeedDataInitializer.AcademicCourseId;

            // 1. Clone system course
            var cloneResp = await client.PostAsync($"/api/v1/courses/{systemId}/clone", null);
            Assert.Equal(HttpStatusCode.OK, cloneResp.StatusCode);
            var cloneJson = await cloneResp.Content.ReadFromJsonAsync<JsonElement>();
            var clonedId = cloneJson.GetProperty("id").GetGuid();

            Assert.Equal("personal", cloneJson.GetProperty("visibility").GetString());
            Assert.False(cloneJson.GetProperty("isSystemCourse").GetBoolean());
            Assert.True(cloneJson.GetProperty("canDelete").GetBoolean());

            // 2. PUT /api/v1/courses/{clonedId} update metadata
            var updateReq = new UpdateCourseRequest(
                Title: "Hệ Điều Hành Thực Chiến 2026",
                Description: "Mô tả khóa học tuỳ chỉnh cho học viên nâng cao.",
                Visibility: "personal",
                IsPublished: true,
                Category: "Khoa học máy tính",
                Level: "advanced",
                LearningOutcomes: new List<string> { "Làm chủ bộ nhớ ảo", "Phân tích Page Fault" },
                Prerequisites: new List<string> { "Nắm vững C/C++" }
            );
            var updateResp = await client.PutAsJsonAsync($"/api/v1/courses/{clonedId}", updateReq);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
            var updatedJson = await updateResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Hệ Điều Hành Thực Chiến 2026", updatedJson.GetProperty("title").GetString());
            Assert.Equal("advanced", updatedJson.GetProperty("level").GetString());

            // 3. DELETE system course -> 403 Forbidden
            var delSystemResp = await client.DeleteAsync($"/api/v1/courses/{systemId}");
            Assert.Equal(HttpStatusCode.Forbidden, delSystemResp.StatusCode);

            // 4. DELETE cloned course -> 200 OK
            var delClonedResp = await client.DeleteAsync($"/api/v1/courses/{clonedId}");
            Assert.Equal(HttpStatusCode.OK, delClonedResp.StatusCode);

            // 5. Subsequent GET on cloned course -> 404 NotFound
            var getDeletedResp = await client.GetAsync($"/api/v1/courses/{clonedId}");
            Assert.Equal(HttpStatusCode.NotFound, getDeletedResp.StatusCode);
        }
    }

    [Fact]
    public async Task LessonFlags_BookmarkAndNeedsReview_PersistWithoutMarkingCompleted()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var lessonId = SeedDataInitializer.LessonPagingId;
            var courseId = SeedDataInitializer.AcademicCourseId;

            // PUT flags: bookmark + needsReview
            var flagsReq = new UpdateLessonFlagsRequest(IsBookmarked: true, NeedsReview: true);
            var flagsResp = await client.PutAsJsonAsync($"/api/v1/lessons/{lessonId}/flags", flagsReq);
            Assert.Equal(HttpStatusCode.OK, flagsResp.StatusCode);
            var flagsJson = await flagsResp.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(flagsJson.GetProperty("isBookmarked").GetBoolean());
            Assert.True(flagsJson.GetProperty("needsReview").GetBoolean());
            Assert.False(flagsJson.GetProperty("isCompleted").GetBoolean());

            // Verify reflected in course summary
            var courseSummary = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
            Assert.True(courseSummary.GetProperty("bookmarkedLessonCount").GetInt32() >= 1);
            Assert.True(courseSummary.GetProperty("needsReviewLessonCount").GetInt32() >= 1);

            // Find lesson inside chapters
            var lessonInCourse = courseSummary.GetProperty("chapters")
                .EnumerateArray()
                .SelectMany(ch => ch.GetProperty("lessons").EnumerateArray())
                .First(l => l.GetProperty("id").GetGuid() == lessonId);

            Assert.True(lessonInCourse.GetProperty("isBookmarked").GetBoolean());
            Assert.True(lessonInCourse.GetProperty("needsReview").GetBoolean());
            Assert.False(lessonInCourse.GetProperty("isCompleted").GetBoolean());
        }
    }

    [Fact]
    public async Task CourseResetProgress_ClearsLessonProgressForUser()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var lessonId = SeedDataInitializer.LessonPagingId;
            var courseId = SeedDataInitializer.AcademicCourseId;

            // Complete a lesson
            var compResp = await client.PostAsync($"/api/v1/lessons/{lessonId}/complete", null);
            Assert.Equal(HttpStatusCode.OK, compResp.StatusCode);

            var courseAfterComp = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
            Assert.True(courseAfterComp.GetProperty("completedLessons").GetInt32() >= 1);

            // Reset progress
            var resetResp = await client.PostAsync($"/api/v1/courses/{courseId}/reset-progress", null);
            Assert.Equal(HttpStatusCode.OK, resetResp.StatusCode);
            var resetJson = await resetResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(courseId, resetJson.GetProperty("courseId").GetGuid());

            // Verify completedLessons == 0
            var courseAfterReset = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
            Assert.Equal(0, courseAfterReset.GetProperty("completedLessons").GetInt32());
        }
    }

    [Fact]
    public async Task InCourseSearch_SupportsUnaccentedVietnameseAndSnippets()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var courseId = SeedDataInitializer.AcademicCourseId;

            // Search with unaccented query "bo nho ao" matching "bộ nhớ ảo"
            var searchResp = await client.GetAsync($"/api/v1/courses/{courseId}/search?q=bo nho ao");
            Assert.Equal(HttpStatusCode.OK, searchResp.StatusCode);

            var searchJson = await searchResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(searchJson.GetProperty("totalMatches").GetInt32() > 0);

            var results = searchJson.GetProperty("results").EnumerateArray().ToList();
            Assert.NotEmpty(results);

            var first = results[0];
            Assert.True(first.TryGetProperty("lessonId", out _));
            Assert.True(first.TryGetProperty("chapterTitle", out _));
            Assert.True(first.TryGetProperty("lessonTitle", out _));
            Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("snippet").GetString()));
            Assert.True(first.GetProperty("matchCount").GetInt32() >= 1);
        }
    }

    [Fact]
    public async Task CourseWorkspaceAssets_ReturnsScopedAssets()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var courseId = SeedDataInitializer.AcademicCourseId;

            var res = await client.GetAsync($"/api/v1/courses/{courseId}/workspace-assets");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(courseId, json.GetProperty("courseId").GetGuid());

            // Verify schema: notebooks, annotations, flashcards, bookmarkedLessons, needsReviewLessons, quizzes, practiceSummary
            Assert.True(json.TryGetProperty("notebooks", out var nbProp) && nbProp.ValueKind == JsonValueKind.Array);
            Assert.True(json.TryGetProperty("annotations", out var annProp) && annProp.ValueKind == JsonValueKind.Array);

            var fcProp = json.GetProperty("flashcards");
            Assert.True(fcProp.TryGetProperty("total", out _));
            Assert.True(fcProp.TryGetProperty("due", out _));
            Assert.True(fcProp.TryGetProperty("mastered", out _));
            Assert.True(fcProp.TryGetProperty("items", out _));

            Assert.True(json.TryGetProperty("bookmarkedLessons", out var bkmProp) && bkmProp.ValueKind == JsonValueKind.Array);
            Assert.True(json.TryGetProperty("needsReviewLessons", out var nrProp) && nrProp.ValueKind == JsonValueKind.Array);
            Assert.True(json.TryGetProperty("quizzes", out var qzProp) && qzProp.ValueKind == JsonValueKind.Array);

            var psProp = json.GetProperty("practiceSummary");
            Assert.True(psProp.TryGetProperty("totalActivities", out _));
            Assert.True(psProp.TryGetProperty("passedActivities", out _));
            Assert.True(psProp.TryGetProperty("averageScore", out _));
        }
    }

    [Fact]
    public async Task CheckDuplicates_And_CleanupDuplicates_SkipsProtectedSystemCourses()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            // 1. Check duplicates on "Hệ điều hành"
            var checkResp = await client.PostAsJsonAsync("/api/v1/courses/check-duplicates",
                new CheckCourseDuplicatesRequest(Title: "Hệ điều hành", Topic: "Bộ nhớ ảo"));
            Assert.Equal(HttpStatusCode.OK, checkResp.StatusCode);

            var checkJson = await checkResp.Content.ReadFromJsonAsync<JsonElement>();
            var dups = checkJson.GetProperty("duplicates").EnumerateArray().ToList();
            Assert.NotEmpty(dups);

            var academicDup = dups.FirstOrDefault(d => d.GetProperty("id").GetGuid() == SeedDataInitializer.AcademicCourseId);
            Assert.True(academicDup.ValueKind != JsonValueKind.Undefined);
            Assert.True(academicDup.GetProperty("isSystemCourse").GetBoolean());

            // 2. Clone to get a non-protected duplicate course
            var cloneResp = await client.PostAsync($"/api/v1/courses/{SeedDataInitializer.AcademicCourseId}/clone", null);
            var cloneJson = await cloneResp.Content.ReadFromJsonAsync<JsonElement>();
            var clonedId = cloneJson.GetProperty("id").GetGuid();

            // 3. Attempt cleanup of both [clonedId, AcademicCourseId]
            var cleanupReq = new CleanupDuplicateCoursesRequest(new List<Guid> { clonedId, SeedDataInitializer.AcademicCourseId });
            var cleanupResp = await client.PostAsJsonAsync("/api/v1/courses/cleanup-duplicates", cleanupReq);
            Assert.Equal(HttpStatusCode.OK, cleanupResp.StatusCode);

            var cleanupJson = await cleanupResp.Content.ReadFromJsonAsync<JsonElement>();
            var deletedIds = cleanupJson.GetProperty("deletedIds").EnumerateArray().Select(x => x.GetGuid()).ToList();
            var skippedIds = cleanupJson.GetProperty("skippedIds").EnumerateArray().Select(x => x.GetGuid()).ToList();

            Assert.Contains(clonedId, deletedIds);
            Assert.Contains(SeedDataInitializer.AcademicCourseId, skippedIds);
        }
    }

    [Fact]
    public async Task LessonAiEnhance_ReturnsStructuredMarkdownContent()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var lessonId = SeedDataInitializer.LessonPagingId;
            var req = new LessonAiEnhanceRequest(
                Action: "expand",
                Prompt: "Giải thích cơ chế Page Fault và phân cấp bộ nhớ."
            );

            var res = await client.PostAsJsonAsync($"/api/v1/lessons/{lessonId}/ai-enhance", req);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(lessonId, json.GetProperty("lessonId").GetGuid());
            Assert.Equal("expand", json.GetProperty("action").GetString());

            var content = json.GetProperty("content").GetString();
            Assert.False(string.IsNullOrWhiteSpace(content), "AI enhanced content should not be empty");
            Assert.True(content.Length > 50, "Content should contain structured Markdown");
        }
    }

    [Fact]
    public async Task Dashboard_CompactMode_OmitsLessonsAndShrinksPayload()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            var fullRes = await client.GetAsync("/api/v1/workspace/dashboard");
            Assert.Equal(HttpStatusCode.OK, fullRes.StatusCode);
            var fullBody = await fullRes.Content.ReadAsStringAsync();

            var compactRes = await client.GetAsync("/api/v1/workspace/dashboard?compact=true");
            Assert.Equal(HttpStatusCode.OK, compactRes.StatusCode);
            var compactBody = await compactRes.Content.ReadAsStringAsync();

            Assert.True(compactBody.Length < fullBody.Length / 2, "Compact dashboard payload should be significantly smaller than full payload");

            var json = JsonSerializer.Deserialize<JsonElement>(compactBody);
            Assert.Equal(0, json.GetProperty("lessons").GetArrayLength());
            var compactCourses = json.GetProperty("courses").EnumerateArray().ToList();
            Assert.True(compactCourses.Count >= 14);
            var firstCompactCourse = compactCourses.First(c => c.GetProperty("lessonCount").GetInt32() > 0);
            Assert.Equal(0, firstCompactCourse.GetProperty("chapters").GetArrayLength());
            Assert.True(firstCompactCourse.GetProperty("chapterCount").GetInt32() > 0);
            Assert.True(firstCompactCourse.TryGetProperty("firstLessonId", out var firstLessonIdEl));
            Assert.NotEqual(JsonValueKind.Null, firstLessonIdEl.ValueKind);
            Assert.True(json.TryGetProperty("documentsCount", out _));
            Assert.True(json.TryGetProperty("notebooksCount", out _));
            Assert.True(json.TryGetProperty("reviewStation", out _));
        }
    }

    [Fact]
    public async Task Courses_PaginationMode_ReturnsSlicesAndPreservesLegacyArray()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            // Legacy array mode
            var legacyRes = await client.GetAsync("/api/v1/courses");
            Assert.Equal(HttpStatusCode.OK, legacyRes.StatusCode);
            var legacyJson = await legacyRes.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, legacyJson.ValueKind);
            var totalLegacy = legacyJson.GetArrayLength();
            Assert.True(totalLegacy >= 14);

            // Paged mode page 1
            var page1Res = await client.GetAsync("/api/v1/courses?limit=8&offset=0");
            Assert.Equal(HttpStatusCode.OK, page1Res.StatusCode);
            var page1Body = await page1Res.Content.ReadAsStringAsync();
            var page1Json = JsonSerializer.Deserialize<JsonElement>(page1Body);
            Assert.Equal(JsonValueKind.Object, page1Json.ValueKind);
            var items1 = page1Json.GetProperty("items").EnumerateArray().ToList();
            Assert.Equal(8, items1.Count);
            Assert.Equal(totalLegacy, page1Json.GetProperty("totalCount").GetInt32());
            Assert.True(page1Json.GetProperty("hasMore").GetBoolean());

            // Compact paged mode page 1
            var compactPage1Res = await client.GetAsync("/api/v1/courses?limit=8&offset=0&compact=true");
            Assert.Equal(HttpStatusCode.OK, compactPage1Res.StatusCode);
            var compactPage1Body = await compactPage1Res.Content.ReadAsStringAsync();
            Assert.True(compactPage1Body.Length < page1Body.Length / 2, "Compact courses slice should be significantly smaller than full slice");
            var compactPage1Json = JsonSerializer.Deserialize<JsonElement>(compactPage1Body);
            var compactItems1 = compactPage1Json.GetProperty("items").EnumerateArray().ToList();
            Assert.Equal(8, compactItems1.Count);
            var sampleCompact = compactItems1.First(c => c.GetProperty("lessonCount").GetInt32() > 0);
            Assert.Equal(0, sampleCompact.GetProperty("chapters").GetArrayLength());
            Assert.True(sampleCompact.GetProperty("chapterCount").GetInt32() > 0);

            // Course detail still returns full chapters & lessons
            var sampleCourseId = sampleCompact.GetProperty("id").GetGuid();
            var detailRes = await client.GetAsync($"/api/v1/courses/{sampleCourseId}");
            Assert.Equal(HttpStatusCode.OK, detailRes.StatusCode);
            var detailJson = await detailRes.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(detailJson.GetProperty("chapters").GetArrayLength() > 0);

            // Paged mode page 2
            var page2Res = await client.GetAsync("/api/v1/courses?limit=8&offset=8");
            Assert.Equal(HttpStatusCode.OK, page2Res.StatusCode);
            var page2Json = await page2Res.Content.ReadFromJsonAsync<JsonElement>();
            var items2 = page2Json.GetProperty("items").EnumerateArray().ToList();
            Assert.NotEmpty(items2);

            var ids1 = items1.Select(x => x.GetProperty("id").GetGuid()).ToHashSet();
            var ids2 = items2.Select(x => x.GetProperty("id").GetGuid()).ToHashSet();
            Assert.Empty(ids1.Intersect(ids2));
        }
    }

    [Fact]
    public async Task PublicCurriculumQuiz_NewStudentCanSubmitWithout403OrMutatingSharedQuiz()
    {
        var (factory, client, connection) = await CreateTestHostAsync();
        await using (factory)
        await using (connection)
        using (client)
        {
            // 1. Default user fetches a public bootcamp quiz id
            var courses = await client.GetFromJsonAsync<JsonElement>("/api/v1/courses");
            var bootcamp = courses.EnumerateArray().First(c => c.GetProperty("slug").GetString() == "doc-course-studxy-fullstack-bootcamp");
            var bootcampId = bootcamp.GetProperty("id").GetGuid();

            var defaultQuizzes = await client.GetFromJsonAsync<JsonElement>($"/api/v1/practice-quizzes?courseId={bootcampId}");
            var sharedQuizId = defaultQuizzes.EnumerateArray().First().GetProperty("id").GetGuid();

            var detail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/practice-quizzes/{sharedQuizId}");
            var firstMcItem = detail.GetProperty("items").EnumerateArray()
                .First(i => i.GetProperty("itemType").GetString() == "multiple_choice");
            var itemId = firstMcItem.GetProperty("id").GetGuid();
            var firstChoiceEl = firstMcItem.GetProperty("choices").EnumerateArray().First();
            var firstChoice = firstChoiceEl.ValueKind == JsonValueKind.String
                ? (firstChoiceEl.GetString() ?? "A")
                : firstChoiceEl.TryGetProperty("id", out var idProp)
                    ? (idProp.GetString() ?? "A")
                    : firstChoiceEl.TryGetProperty("key", out var keyProp)
                        ? (keyProp.GetString() ?? "A")
                        : firstChoiceEl.ToString();

            // 2. Register a real new student user
            var regRes = await client.PostAsJsonAsync("/api/v1/auth/register", new
            {
                email = $"quiz.student.{Guid.NewGuid():N}@studxy.vn",
                password = "StudxyTestPassword!2026",
                fullName = "Quiz Student"
            });
            Assert.True(regRes.IsSuccessStatusCode, $"Register expected 2xx, got {(int)regRes.StatusCode}");
            var regJson = await regRes.Content.ReadFromJsonAsync<JsonElement>();
            var studentId = regJson.GetProperty("user").GetProperty("id").GetGuid();

            client.DefaultRequestHeaders.Remove("X-Test-User-Id");
            client.DefaultRequestHeaders.Add("X-Test-User-Id", studentId.ToString());

            // 3. Submit single item directly against sharedQuizId as new student -> must return 200 (not 403)
            var submitRes = await client.PostAsJsonAsync($"/api/v1/practice-quizzes/{sharedQuizId}/items/{itemId}/submit",
                new SubmitQuizItemRequest(Answer: firstChoice, IdempotencyKey: Guid.NewGuid(), WaitForAiCompletion: false));
            Assert.Equal(HttpStatusCode.OK, submitRes.StatusCode);

            // 4. Submit-all directly against sharedQuizId as new student -> must return 200 (not 403) and not mutate shared quiz
            var submitAllRes = await client.PostAsJsonAsync($"/api/v1/practice-quizzes/{sharedQuizId}/submit-all",
                new
                {
                    answers = new[] { new { itemId, answer = firstChoice } },
                    durationSeconds = 12
                });
            Assert.Equal(HttpStatusCode.OK, submitAllRes.StatusCode);

            // 5. Verify shared quiz still un-mutated for default user
            client.DefaultRequestHeaders.Remove("X-Test-User-Id");
            var defaultDetail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/practice-quizzes/{sharedQuizId}");
            var defaultItem = defaultDetail.GetProperty("items").EnumerateArray().First(i => i.GetProperty("id").GetGuid() == itemId);
            Assert.Equal(JsonValueKind.Null, defaultItem.GetProperty("evaluatedAt").ValueKind);
        }
    }
}
