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

public class BootcampAndQuizUpgradeTests
{
    private static WebApplicationFactory<Program> CreateRealSqliteFactory(SqliteConnection openConnection, string? aiProxyBaseUrl = null)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                if (!string.IsNullOrWhiteSpace(aiProxyBaseUrl))
                {
                    builder.UseSetting("AiProxy:BaseUrl", aiProxyBaseUrl);
                }
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
    public void BootcampPracticeActivityCatalog_GeneratesAccurateLanguageActivities_AndRejectsAsciiArt()
    {
        var courseId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var cases = new (string RelPath, string Title, string ExpectedLang, string ExpectedFillCat)[]
        {
            ("phase-1-cs-foundations/lesson-01-problem-solving/README.md", "Bài 01: Tư duy giải quyết vấn đề", "javascript", "react"),
            ("phase-2-web-foundations/lesson-06-css-flexbox/README.md", "Bài 06: CSS Flexbox", "css", "react"),
            ("phase-5-backend-dotnet/lesson-07-dependency-injection/README.md", "Bài 07: Dependency Injection", "csharp", "dotnet"),
            ("phase-6-database-sql/lesson-03-sql-joins/README.md", "Bài 03: SQL JOINs", "sql", "sql")
        };

        foreach (var (relPath, title, expectedLang, expectedCat) in cases)
        {
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                ChapterId = Guid.NewGuid(),
                Title = title,
                Slug = "test-lesson",
                SortOrder = 1
            };

            var sampleAsciiMd = """
                # Test
                ```text
                +---+---+
                | A | B |
                +---+---+
                ```
                ```mermaid
                flowchart TD
                  A --> B
                ```
                """;

            var activities = BootcampPracticeActivityCatalog.BuildForLesson(relPath, courseId, lesson, sampleAsciiMd, now);

            Assert.Equal(3, activities.Count);
            Assert.Equal(101, activities[0].OrderIndex);
            Assert.Equal("fill_code", activities[0].Type);
            Assert.Equal(102, activities[1].OrderIndex);
            Assert.Equal("predict_output", activities[1].Type);
            Assert.Equal(103, activities[2].OrderIndex);
            Assert.Equal("bug_fix", activities[2].Type);

            Assert.Equal(SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "fill_code", 101), activities[0].Id);
            Assert.Equal(SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "predict_output", 102), activities[1].Id);
            Assert.Equal(SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "bug_fix", 103), activities[2].Id);

            foreach (var act in activities)
            {
                Assert.DoesNotContain("+---+", act.ConfigJson, StringComparison.Ordinal);
                Assert.DoesNotContain("|---|", act.ConfigJson, StringComparison.Ordinal);
                Assert.DoesNotContain("┌", act.ConfigJson, StringComparison.Ordinal);
                Assert.DoesNotContain("flowchart TD", act.ConfigJson, StringComparison.Ordinal);

                using var cfgDoc = JsonDocument.Parse(act.ConfigJson);
                var item = cfgDoc.RootElement.GetProperty("items")[0];
                Assert.Equal(expectedLang, item.GetProperty("language").GetString());
                if (act.Type == "fill_code")
                {
                    Assert.Equal(expectedCat, item.GetProperty("category").GetString());
                    Assert.True(item.GetProperty("codeTemplate").GetArrayLength() > 0);
                    Assert.True(item.GetProperty("blanks").GetArrayLength() > 0);
                    Assert.True(item.GetProperty("lineExplanations").GetArrayLength() > 0);
                }
                else if (act.Type == "predict_output")
                {
                    Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("code").GetString()));
                    Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("expectedOutput").GetString()));
                    Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("explanation").GetString()));
                }
                else if (act.Type == "bug_fix")
                {
                    Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("buggyCode").GetString()));
                    Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("cleanFixedCode").GetString()));
                    Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("explanation").GetString()));
                }
            }
        }

        Assert.True(BootcampPracticeActivityCatalog.IsAsciiOrDiagramBlock("text", "anything"));
        Assert.True(BootcampPracticeActivityCatalog.IsAsciiOrDiagramBlock("mermaid", "graph TD; A-->B;"));
        Assert.True(BootcampPracticeActivityCatalog.IsAsciiOrDiagramBlock("javascript", "+---+\n| A |\n+---+"));
        Assert.False(BootcampPracticeActivityCatalog.IsAsciiOrDiagramBlock("javascript", "const x = 1;"));
    }

    [Fact]
    public void TryNormalizeRelativePath_ExtractsCourseRelativePathFromFileUri()
    {
        var ok1 = DocumentCourseSyncService.TryNormalizeRelativePath(
            null,
            "file:///D:/03_Study/Studxy/course/phase-1-cs-foundations/lesson-01-problem-solving/README.md",
            out var norm1,
            out _);

        Assert.True(ok1);
        Assert.Equal("phase-1-cs-foundations/lesson-01-problem-solving/README.md", norm1);

        var ok2 = DocumentCourseSyncService.TryNormalizeRelativePath(
            "phase-2-web-foundations/lesson-01-how-web-works/README.md",
            "http://localhost:3000/file:/D:/03_Study/Studxy/course/phase-1-cs-foundations/lesson-01-problem-solving/README.md#quiz",
            out var norm2,
            out var anchor2);

        Assert.True(ok2);
        Assert.Equal("phase-1-cs-foundations/lesson-01-problem-solving/README.md", norm2);
        Assert.Equal("quiz", anchor2);

        // Traversal outside course root must still be blocked
        var bad = DocumentCourseSyncService.TryNormalizeRelativePath(
            null,
            "file:///D:/03_Study/Studxy/course/../../backend/secrets.json",
            out _,
            out _);
        Assert.False(bad);
    }

    [Fact]
    public void ParseQuizMarkdown_ExtractsExplanationsFromAllDetailsFormats()
    {
        var now = DateTimeOffset.UtcNow;

        // Format A: Real Bootcamp table <details> (phase-1 lesson-01)
        var lesson1Md = File.ReadAllText(Path.Combine(
            DocumentCourseSyncService.DefaultStudxyBootcampRootPath,
            "phase-1-cs-foundations",
            "lesson-01-problem-solving",
            "README.md"));
        var quizIdx = lesson1Md.IndexOf("## 🧪 Quiz", StringComparison.Ordinal);
        Assert.True(quizIdx >= 0);
        var section = lesson1Md[quizIdx..];
        var detailsStart = section.IndexOf("<details", StringComparison.OrdinalIgnoreCase);
        Assert.True(detailsStart >= 0);

        var tableItems = CourseWizardAndImportService.ParseQuizMarkdown(
            Guid.NewGuid(),
            section[..detailsStart],
            section[detailsStart..],
            now);
        Assert.Equal(8, tableItems.Count);
        Assert.Equal("B", tableItems[0].ClozeSolution);
        var exp1 = JsonDocument.Parse(tableItems[0].PromptAstJson).RootElement.GetProperty("explanation").GetString();
        Assert.False(string.IsNullOrWhiteSpace(exp1));
        Assert.Contains("Decomposition", exp1, StringComparison.OrdinalIgnoreCase);

        // Format B: Numbered list & bullet list in <details>
        var listQuizMd = """
            #### Câu 1: Chọn kết quả đúng:
            A. Option A
            B. Option B
            C. Option C
            D. Option D

            #### Câu 2: Chọn phát biểu chuẩn:
            A. Sai
            B. Đúng
            C. Sai
            D. Sai
            """;
        var listAnswerKey = """
            <details>
            <summary>Đáp án</summary>

            1. **Đáp án C:** Giải thích chi tiết cho câu 1 với `const`.
            2. **B** – Giải thích chi tiết cho câu 2 về `let`.
            </details>
            """;

        var listItems = CourseWizardAndImportService.ParseQuizMarkdown(Guid.NewGuid(), listQuizMd, listAnswerKey, now);
        Assert.Equal(2, listItems.Count);
        Assert.Equal("C", listItems[0].ClozeSolution);
        Assert.Equal("B", listItems[1].ClozeSolution);
        Assert.Contains("Giải thích chi tiết cho câu 1", JsonDocument.Parse(listItems[0].PromptAstJson).RootElement.GetProperty("explanation").GetString());
        Assert.Contains("Giải thích chi tiết cho câu 2", JsonDocument.Parse(listItems[1].PromptAstJson).RootElement.GetProperty("explanation").GetString());

        // Format C: Inline > **Giải thích:** inside <details>
        var inlineQuizMd = """
            ### Câu 1: Vì sao dùng AsNoTracking?
            - A. Tắt ChangeTracker cho truy vấn read-only
            - B. Bật tracking
            - C. Xóa dữ liệu
            - D. Tạo index

            <details>
            <summary>Xem đáp án</summary>

            **Đáp án đúng: A.**
            > **Giải thích:** AsNoTracking giảm 50-70% bộ nhớ RAM khi chỉ đọc.
            </details>
            """;
        var inlineItems = CourseWizardAndImportService.ParseQuizMarkdown(Guid.NewGuid(), inlineQuizMd, null, now);
        Assert.Single(inlineItems);
        Assert.Equal("A", inlineItems[0].ClozeSolution);
        Assert.Contains("AsNoTracking giảm 50-70%", JsonDocument.Parse(inlineItems[0].PromptAstJson).RootElement.GetProperty("explanation").GetString());
    }

    [Fact]
    public async Task PublicLessonCompleteAutoEnrolls_QuizGenerateAddsPrefix_AndSubmitAllSavesAttemptHistory()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn, aiProxyBaseUrl: "http://127.0.0.1:1");
        using var defaultClient = factory.CreateClient();

        // Register a brand new student (never manually enrolled in public courses)
        var regEmail = $"wp1.{Guid.NewGuid():N}@studxy.vn";
        var regResp = await defaultClient.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(regEmail, "Studxy@2026", "WP1 Learner"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var token = (await regResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;

        using var studentClient = factory.CreateClient();
        studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Pick a public published course (EnglishCourseId) + create a private course owned by DefaultUserId
        Guid privateLessonId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var privCourse = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = SeedDataInitializer.DefaultUserId,
                Title = "Private Course WP1",
                Slug = $"priv-wp1-{Guid.NewGuid():N}",
                Visibility = "private",
                IsPublished = false
            };
            var privChap = new Chapter { Id = Guid.NewGuid(), CourseId = privCourse.Id, Title = "Ch1", SortOrder = 1 };
            var privLes = new Lesson { Id = Guid.NewGuid(), ChapterId = privChap.Id, Title = "Secret Lesson", Slug = "secret", SortOrder = 1, IsPublished = true };
            db.Courses.Add(privCourse);
            db.Chapters.Add(privChap);
            db.Lessons.Add(privLes);
            await db.SaveChangesAsync();
            privateLessonId = privLes.Id;
        }

        // 1. POST /lessons/{id}/complete on public published course auto-enrolls & returns 200 OK immediately
        var completePubResp = await studentClient.PostAsync($"/api/v1/lessons/{SeedDataInitializer.EnglishLesson11Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completePubResp.StatusCode);

        // Private course remains 403 Forbidden
        var completePrivResp = await studentClient.PostAsync($"/api/v1/lessons/{privateLessonId}/complete", null);
        Assert.Equal(HttpStatusCode.Forbidden, completePrivResp.StatusCode);

        // 2. POST /courses/{id}/quizzes/generate prefixes "[Đề Tự Tạo] " when missing
        var genResp = await studentClient.PostAsJsonAsync(
            $"/api/v1/courses/{SeedDataInitializer.AcademicCourseId}/quizzes/generate",
            new GenerateCourseQuizRequest(Count: 4, Title: "Ôn Nhanh Paging"));
        Assert.Equal(HttpStatusCode.Created, genResp.StatusCode);
        var genJson = await genResp.Content.ReadFromJsonAsync<JsonElement>();
        var genQuizId = genJson.GetProperty("id").GetGuid();
        Assert.StartsWith("[Đề Tự Tạo] Ôn Nhanh Paging", genJson.GetProperty("title").GetString());

        // 3. POST /submit-all records PracticeAttempt and returns savedAttemptId + attemptHistory + explanation
        var quizDetail = await studentClient.GetFromJsonAsync<JsonElement>($"/api/v1/practice-quizzes/{genQuizId}");
        var answers = new List<SubmitAllQuizItemEntry>();
        foreach (var item in quizDetail.GetProperty("items").EnumerateArray())
        {
            answers.Add(new SubmitAllQuizItemEntry(item.GetProperty("id").GetGuid(), "A"));
        }

        var submitAllResp = await studentClient.PostAsJsonAsync(
            $"/api/v1/practice-quizzes/{genQuizId}/submit-all",
            new SubmitAllQuizItemsRequest(answers, DurationSeconds: 32));
        Assert.Equal(HttpStatusCode.OK, submitAllResp.StatusCode);
        var submitAllJson = await submitAllResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(submitAllJson.TryGetProperty("savedAttemptId", out var savedAttemptProp));
        Assert.NotEqual(Guid.Empty, savedAttemptProp.GetGuid());
        Assert.Equal(1, submitAllJson.GetProperty("attemptHistory").GetArrayLength());

        var quizDetailAfter = await studentClient.GetFromJsonAsync<JsonElement>($"/api/v1/practice-quizzes/{genQuizId}");
        Assert.Equal(1, quizDetailAfter.GetProperty("attemptHistory").GetArrayLength());
        Assert.Equal(32, quizDetailAfter.GetProperty("attemptHistory")[0].GetProperty("durationSeconds").GetInt32());

        // 4. Language-aware static fallback on /ai/explain for SQL (must not suggest C# try-catch/IDisposable)
        var aiSqlResp = await studentClient.PostAsJsonAsync("/api/v1/ai/explain",
            new AiInSituRequest("SELECT u.id FROM users u LEFT JOIN orders o ON o.user_id = u.id;", "SQL JOINs", "explain", "sql"));
        Assert.Equal(HttpStatusCode.OK, aiSqlResp.StatusCode);
        var aiSqlText = (await aiSqlResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("explanation").GetString() ?? "";
        Assert.Contains("SQL", aiSqlText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IDisposable", aiSqlText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("try-catch", aiSqlText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BootcampSync_And_QuizCountDeduplication_And_PedagogicalSort_WorkEndToEnd()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn, aiProxyBaseUrl: "http://127.0.0.1:1");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var report = await DocumentCourseSyncService.SyncStudxyBootcampCourseAsync(db);
            Assert.Equal(1, report.SyncedCourses);
            Assert.True(report.CreatedLessons >= 140);
            Assert.True(report.CreatedQuizzes >= 120);
            Assert.True(report.CreatedFlashcards >= 150);

            // Verify PostgreSQL MaxLength constraints compliance across all synced entities
            var allTags = await db.Tags.ToListAsync();
            Assert.NotEmpty(allTags);
            Assert.All(allTags, t =>
            {
                Assert.True(t.Name.Length <= 50, $"Tag.Name '{t.Name}' exceeds 50 chars ({t.Name.Length})");
                Assert.True(t.ColorKey.Length <= 20, $"Tag.ColorKey '{t.ColorKey}' exceeds 20 chars");
            });

            var allActivities = await db.PracticeActivities.ToListAsync();
            Assert.NotEmpty(allActivities);
            Assert.All(allActivities, pa =>
            {
                Assert.True(pa.Title.Length <= 255, $"PracticeActivity.Title '{pa.Title}' exceeds 255 chars ({pa.Title.Length})");
                Assert.True(pa.Type.Length <= 50, $"PracticeActivity.Type '{pa.Type}' exceeds 50 chars");
            });

            var allQuizzes = await db.PracticeQuizzes.ToListAsync();
            Assert.NotEmpty(allQuizzes);
            Assert.All(allQuizzes, pq =>
            {
                Assert.True(pq.Title.Length <= 255, $"PracticeQuiz.Title '{pq.Title}' exceeds 255 chars ({pq.Title.Length})");
                Assert.True(pq.Status.Length <= 30, $"PracticeQuiz.Status '{pq.Status}' exceeds 30 chars");
            });

            var allChapters = await db.Chapters.ToListAsync();
            Assert.NotEmpty(allChapters);
            Assert.All(allChapters, ch =>
            {
                Assert.True(ch.Title.Length <= 255, $"Chapter.Title '{ch.Title}' exceeds 255 chars ({ch.Title.Length})");
            });

            var allLessons = await db.Lessons.ToListAsync();
            Assert.NotEmpty(allLessons);
            Assert.All(allLessons, l =>
            {
                Assert.True(l.Title.Length <= 255, $"Lesson.Title '{l.Title}' exceeds 255 chars ({l.Title.Length})");
                Assert.True(l.Slug.Length <= 255, $"Lesson.Slug '{l.Slug}' exceeds 255 chars ({l.Slug.Length})");
            });

            var allCards = await db.Flashcards.ToListAsync();
            Assert.NotEmpty(allCards);
            Assert.All(allCards, fc =>
            {
                Assert.True(fc.CardType.Length <= 30, $"Flashcard.CardType '{fc.CardType}' exceeds 30 chars");
            });
        }

        using var defaultClient = factory.CreateClient();
        var regEmail = $"bc.{Guid.NewGuid():N}@studxy.vn";
        var regResp = await defaultClient.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(regEmail, "Studxy@2026", "Bootcamp Learner"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var token = (await regResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;

        using var regClient = factory.CreateClient();
        regClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var courses = await regClient.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        Assert.NotNull(courses);
        var bootcamp = courses.First(c => c.GetProperty("slug").GetString() == DocumentCourseSyncService.StudxyBootcampSlug);
        var bootcampId = bootcamp.GetProperty("id").GetGuid();
        var courseQuizCount = bootcamp.GetProperty("quizCount").GetInt32();

        // GET /api/v1/practice-quizzes triggers cloning for registered user; quizCount on Course Hub MUST match without 2x duplication
        var regQuizzes = await regClient.GetFromJsonAsync<List<JsonElement>>($"/api/v1/practice-quizzes?courseId={bootcampId}");
        Assert.NotNull(regQuizzes);
        Assert.Equal(courseQuizCount, regQuizzes.Count);

        // Pedagogical order: lesson/chapter ordered by chapterSortOrder, lesson before chapter, final at the very end
        Assert.Equal("final", regQuizzes[^1].GetProperty("category").GetString());
        var ch3Idx = regQuizzes.FindIndex(q => q.GetProperty("category").GetString() == "chapter");
        Assert.True(ch3Idx > 0);
        Assert.Equal("lesson", regQuizzes[0].GetProperty("category").GetString());

        // Refresh /api/v1/courses after learner clone and verify quizCount still equals regQuizzes.Count (not 2x!)
        var coursesAfterClone = await regClient.GetFromJsonAsync<List<JsonElement>>("/api/v1/courses");
        var bootcampAfter = coursesAfterClone!.First(c => c.GetProperty("id").GetGuid() == bootcampId);
        Assert.Equal(regQuizzes.Count, bootcampAfter.GetProperty("quizCount").GetInt32());

        // by-path with file:///.../course/... resolves cleanly to Lesson 01
        var byPathResp = await regClient.GetAsync(
            $"/api/v1/lessons/by-path?courseId={bootcampId}&relativePath={Uri.EscapeDataString("file:///D:/03_Study/Studxy/course/phase-1-cs-foundations/lesson-01-problem-solving/README.md")}");
        Assert.Equal(HttpStatusCode.OK, byPathResp.StatusCode);
        var byPathJson = await byPathResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(byPathJson.GetProperty("found").GetBoolean());
        Assert.Equal("phase-1-cs-foundations/lesson-01-problem-solving/README.md", byPathJson.GetProperty("sourceRelativePath").GetString());
    }

    [Fact]
    public void PostgresMaxLengthCompliance_CatalogAndCourseTagName_NeverExceedLimits()
    {
        // 1. CourseGenerationService.BuildCourseTagName with very long title
        var longDocTitle = "Khóa học Phát triển Hệ thống Web & Cloud Native với Kiến trúc Microservices và Trí tuệ Nhân tạo Đột phá 2026";
        var tagName = CourseGenerationService.BuildCourseTagName(longDocTitle, Guid.NewGuid());
        Assert.True(tagName.Length <= 50, $"Course tag name '{tagName}' must not exceed 50 chars");

        // 2. BootcampPracticeActivityCatalog with extremely long lesson title
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            ChapterId = Guid.NewGuid(),
            Title = new string('A', 300),
            Slug = "long-lesson",
            SortOrder = 1
        };
        var acts = BootcampPracticeActivityCatalog.BuildForLesson(
            "phase-1-cs-foundations/lesson-01-problem-solving/README.md",
            Guid.NewGuid(),
            lesson,
            "# Lesson\nContent",
            DateTimeOffset.UtcNow);

        Assert.NotEmpty(acts);
        Assert.All(acts, a =>
        {
            Assert.True(a.Title.Length <= 255, $"Activity title '{a.Title}' must not exceed 255 chars");
            Assert.True(a.Type.Length <= 50, $"Activity type '{a.Type}' must not exceed 50 chars");
        });
    }
}
