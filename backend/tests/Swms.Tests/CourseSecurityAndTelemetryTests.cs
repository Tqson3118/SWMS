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
using Xunit;

namespace Swms.Tests;

public class CourseSecurityAndTelemetryTests
{
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

    private static async Task<(string Token, Guid UserId, Guid PersonalCourseId)> RegisterUserAsync(
        HttpClient client,
        string prefix,
        string fullName)
    {
        var resp = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest($"{prefix}.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", fullName));
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return (
            json.GetProperty("accessToken").GetString()!,
            json.GetProperty("user").GetProperty("id").GetGuid(),
            json.GetProperty("personalWorkspaceCourseId").GetGuid()
        );
    }

    private static async Task<HttpResponseMessage> SendWithBearerAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        string token,
        object? body = null)
    {
        using var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body != null)
        {
            req.Content = JsonContent.Create(body);
        }
        return await client.SendAsync(req);
    }

    [Fact]
    public async Task PrivatePersonalAndUnpublishedCourses_BlockEnrollAndRead_EvenForDefaultUserAndStaleEnrollment()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (tokenA, userAId, personalCourseAId) = await RegisterUserAsync(client, "sec.owner", "Owner A");
        var (tokenB, userBId, _) = await RegisterUserAsync(client, "sec.other", "Learner B");

        Guid privateCourseId;
        Guid unpublishedPublicCourseId;
        Guid privateLessonId;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

            var privateCourse = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = userAId,
                Title = "Khóa học riêng tư của A",
                Slug = $"priv-course-{Guid.NewGuid():N}",
                Visibility = "private",
                IsPublished = true
            };
            var unpublishedCourse = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = userAId,
                Title = "Khóa học chưa xuất bản của A",
                Slug = $"draft-course-{Guid.NewGuid():N}",
                Visibility = "public",
                IsPublished = false
            };
            db.Courses.AddRange(privateCourse, unpublishedCourse);

            var privCh = new Chapter
            {
                Id = Guid.NewGuid(),
                CourseId = privateCourse.Id,
                Title = "Private Chapter",
                SortOrder = 1
            };
            db.Chapters.Add(privCh);

            var privLesson = new Lesson
            {
                Id = Guid.NewGuid(),
                ChapterId = privCh.Id,
                Title = "Bài học mật",
                Slug = $"priv-lesson-{Guid.NewGuid():N}",
                SortOrder = 1,
                ContentAstJson = "{\"meta\":{\"sourceFolder\":\"SecretCourse\",\"sourceRelativePath\":\"intro.md\"},\"content\":[]}",
                PlainText = "Secret private content",
                IsPublished = true
            };
            db.Lessons.Add(privLesson);
            await db.SaveChangesAsync();

            privateCourseId = privateCourse.Id;
            unpublishedPublicCourseId = unpublishedCourse.Id;
            privateLessonId = privLesson.Id;
        }

        // 1. User B cannot enroll in User A's personal, private, or unpublished courses -> 403
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{personalCourseAId}/enroll", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{privateCourseId}/enroll", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{unpublishedPublicCourseId}/enroll", tokenB)).StatusCode);

        // 2. User B cannot read User A's personal, private, or unpublished courses via GET /courses/{id} or /lessons/by-path -> 403
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{personalCourseAId}", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{privateCourseId}", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{unpublishedPublicCourseId}", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/lessons/by-path?courseId={privateCourseId}&relativePath=intro.md", tokenB)).StatusCode);

        // 3. DefaultUserId (no bearer -> resolves to DefaultUserId under test host) CANNOT read/enroll User A's personal/private/unpublished courses
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/v1/courses/{personalCourseAId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/v1/courses/{privateCourseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/v1/courses/{unpublishedPublicCourseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/api/v1/courses/{privateCourseId}/enroll", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/v1/lessons/by-path?courseId={privateCourseId}&relativePath=intro.md")).StatusCode);

        // 4. User B CANNOT read or enroll in DefaultUserId's PersonalCourseId -> 403
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{SeedDataInitializer.PersonalCourseId}", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{SeedDataInitializer.PersonalCourseId}/enroll", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{SeedDataInitializer.PersonalCourseId}/progress", tokenB)).StatusCode);

        // 5. Even if a stale CourseEnrollment exists in DB for User B on private/personal course, User B is STILL denied 403
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            db.CourseEnrollments.AddRange(
                new CourseEnrollment { UserId = userBId, CourseId = privateCourseId, Role = "student" },
                new CourseEnrollment { UserId = userBId, CourseId = personalCourseAId, Role = "student" }
            );
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{privateCourseId}", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{privateCourseId}/progress", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/lessons/{privateLessonId}", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/lessons/by-path?courseId={privateCourseId}&relativePath=intro.md", tokenB)).StatusCode);

        var bLessonsResp = await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/lessons", tokenB);
        Assert.Equal(HttpStatusCode.OK, bLessonsResp.StatusCode);
        var bLessonsBody = await bLessonsResp.Content.ReadAsStringAsync();
        Assert.DoesNotContain(privateLessonId.ToString(), bLessonsBody, StringComparison.OrdinalIgnoreCase);

        // 6. Owner A can still read their own private course & User B can still enroll/read a public published course
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{privateCourseId}", tokenA)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{SeedDataInitializer.AcademicCourseId}/enroll", tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{SeedDataInitializer.AcademicCourseId}", tokenB)).StatusCode);
    }

    [Fact]
    public async Task DraftLessons_AreHiddenFromNonOwners_InProgressAndLessonsEndpoints_WhileVisibleToOwner()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (tokenOwner, ownerId, _) = await RegisterUserAsync(client, "draft.owner", "Course Owner");
        var (tokenLearner, learnerId, _) = await RegisterUserAsync(client, "draft.learner", "Course Learner");

        Guid courseId, pubLesson1Id, pubLesson2Id, draftLessonId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = ownerId,
                Title = "Khóa học công khai có bài nháp",
                Slug = $"pub-draft-test-{Guid.NewGuid():N}",
                Visibility = "public",
                IsPublished = true
            };
            var ch = new Chapter { Id = Guid.NewGuid(), CourseId = course.Id, Title = "Chương 1", SortOrder = 1 };
            var l1 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Bài 1 (Published)", Slug = $"l1-{Guid.NewGuid():N}", SortOrder = 1, PlainText = "Published 1", IsPublished = true };
            var l2 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Bài 2 (Published)", Slug = $"l2-{Guid.NewGuid():N}", SortOrder = 2, PlainText = "Published 2", IsPublished = true };
            var l3 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Bài 3 (Draft)", Slug = $"l3-{Guid.NewGuid():N}", SortOrder = 3, PlainText = "Draft secret", IsPublished = false };

            db.Courses.Add(course);
            db.Chapters.Add(ch);
            db.Lessons.AddRange(l1, l2, l3);
            await db.SaveChangesAsync();

            courseId = course.Id;
            pubLesson1Id = l1.Id;
            pubLesson2Id = l2.Id;
            draftLessonId = l3.Id;
        }

        // Learner enrolls and completes published lesson 1
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{courseId}/enroll", tokenLearner)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{pubLesson1Id}/complete", tokenLearner)).StatusCode);

        // Even if learner somehow has a historical progress record on the draft lesson, it must NOT count
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            db.UserLessonProgresses.Add(new UserLessonProgress
            {
                UserId = learnerId,
                LessonId = draftLessonId,
                IsCompleted = true,
                CompletedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Learner GET /progress -> sees only 2 published lessons, 1 completed
        var learnerProgResp = await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{courseId}/progress", tokenLearner);
        Assert.Equal(HttpStatusCode.OK, learnerProgResp.StatusCode);
        var learnerProg = await learnerProgResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, learnerProg.GetProperty("totalLessons").GetInt32());
        Assert.Equal(1, learnerProg.GetProperty("completedLessons").GetInt32());
        Assert.Equal(50, learnerProg.GetProperty("percentCompleted").GetInt32());
        var completedIds = learnerProg.GetProperty("completedLessonIds").EnumerateArray().Select(e => e.GetGuid()).ToList();
        Assert.Contains(pubLesson1Id, completedIds);
        Assert.DoesNotContain(draftLessonId, completedIds);

        // Learner GET /lessons -> includes pubLesson1 & pubLesson2, excludes draftLesson
        var learnerLessonsResp = await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/lessons", tokenLearner);
        Assert.Equal(HttpStatusCode.OK, learnerLessonsResp.StatusCode);
        var learnerLessonsText = await learnerLessonsResp.Content.ReadAsStringAsync();
        Assert.Contains(pubLesson1Id.ToString(), learnerLessonsText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(pubLesson2Id.ToString(), learnerLessonsText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(draftLessonId.ToString(), learnerLessonsText, StringComparison.OrdinalIgnoreCase);

        // Learner GET /lessons/{draftLessonId} -> 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/lessons/{draftLessonId}", tokenLearner)).StatusCode);

        // Owner still sees all 3 lessons in /progress, /lessons, and GET /lessons/{draftLessonId}
        var ownerProg = await (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{courseId}/progress", tokenOwner)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, ownerProg.GetProperty("totalLessons").GetInt32());

        var ownerLessonsText = await (await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/lessons", tokenOwner)).Content.ReadAsStringAsync();
        Assert.Contains(draftLessonId.ToString(), ownerLessonsText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/lessons/{draftLessonId}", tokenOwner)).StatusCode);
    }

    [Fact]
    public async Task LessonCompletion_CalculatesRealStreakDays_NotHardcodedToTwo()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (token, userId, personalCourseId) = await RegisterUserAsync(client, "streak.real", "Streak Learner");

        Guid lesson1Id, lesson2Id, cardId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var ch = new Chapter { Id = Guid.NewGuid(), CourseId = personalCourseId, Title = "Ch1", SortOrder = 1 };
            var l1 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Streak L1", Slug = $"sl1-{Guid.NewGuid():N}", SortOrder = 1, PlainText = "Content 1", IsPublished = true };
            var l2 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Streak L2", Slug = $"sl2-{Guid.NewGuid():N}", SortOrder = 2, PlainText = "Content 2", IsPublished = true };
            var card = new Flashcard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = l1.Id,
                CardType = "standard",
                FrontAstJson = "{\"text\":\"Front\"}",
                BackAstJson = "{\"text\":\"Back\"}",
                DueAt = DateTimeOffset.UtcNow
            };
            db.Chapters.Add(ch);
            db.Lessons.AddRange(l1, l2);
            db.Flashcards.Add(card);
            await db.SaveChangesAsync();

            lesson1Id = l1.Id;
            lesson2Id = l2.Id;
            cardId = card.Id;
        }

        // 1. User syncs 1 FSRS review TODAY + completes 1 lesson TODAY -> StreakDays MUST be 1 (NOT hardcoded 2!)
        var syncResp = await SendWithBearerAsync(
            client,
            HttpMethod.Post,
            "/api/v1/flashcards/sync-reviews",
            token,
            new SyncReviewsBatchRequest([new FsrsReviewEventInput(Guid.NewGuid(), cardId, 3, DateTimeOffset.UtcNow, 1200)]));
        Assert.Equal(HttpStatusCode.OK, syncResp.StatusCode);

        var complete1Resp = await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{lesson1Id}/complete", token);
        Assert.Equal(HttpStatusCode.OK, complete1Resp.StatusCode);

        var meAfterDay1 = await (await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/auth/me", token)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, meAfterDay1.GetProperty("streakDays").GetInt32());

        // 2. Syncing another review TODAY must STILL keep StreakDays == 1
        var syncResp2 = await SendWithBearerAsync(
            client,
            HttpMethod.Post,
            "/api/v1/flashcards/sync-reviews",
            token,
            new SyncReviewsBatchRequest([new FsrsReviewEventInput(Guid.NewGuid(), cardId, 3, DateTimeOffset.UtcNow, 1100)]));
        Assert.Equal(HttpStatusCode.OK, syncResp2.StatusCode);

        var meStillDay1 = await (await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/auth/me", token)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, meStillDay1.GetProperty("streakDays").GetInt32());

        // 3. Seed real historical activity for YESTERDAY -> completing lesson2 TODAY gives StreakDays == 2
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            db.FlashcardReviews.Add(new FlashcardReview
            {
                Id = Guid.NewGuid(),
                ReviewEventId = Guid.NewGuid(),
                FlashcardId = cardId,
                UserId = userId,
                Rating = 3,
                ReviewedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });
            await db.SaveChangesAsync();
        }

        var complete2Resp = await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{lesson2Id}/complete", token);
        Assert.Equal(HttpStatusCode.OK, complete2Resp.StatusCode);

        var meAfterDay2 = await (await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/auth/me", token)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, meAfterDay2.GetProperty("streakDays").GetInt32());
    }

    [Fact]
    public async Task TelemetryAndResumeState_TracksTimeScrollOffsetAndResumesLatestLesson()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (_, ownerId, ownerPersonalCourseId) = await RegisterUserAsync(client, "tel.owner", "Telemetry Owner");
        var (tokenLearner, _, _) = await RegisterUserAsync(client, "tel.learner", "Telemetry Learner");

        Guid courseId, lesson1Id, lesson2Id, draftLessonId, privateLessonId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = ownerId,
                Title = "Telemetry Course",
                Slug = $"tel-course-{Guid.NewGuid():N}",
                Visibility = "public",
                IsPublished = true
            };
            var ch = new Chapter { Id = Guid.NewGuid(), CourseId = course.Id, Title = "Ch1", SortOrder = 1 };
            var l1 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Lesson 1", Slug = $"tl1-{Guid.NewGuid():N}", SortOrder = 1, PlainText = "L1", IsPublished = true };
            var l2 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Lesson 2", Slug = $"tl2-{Guid.NewGuid():N}", SortOrder = 2, PlainText = "L2", IsPublished = true };
            var l3 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch.Id, Title = "Lesson 3 Draft", Slug = $"tl3-{Guid.NewGuid():N}", SortOrder = 3, PlainText = "L3", IsPublished = false };

            var privCh = new Chapter { Id = Guid.NewGuid(), CourseId = ownerPersonalCourseId, Title = "PrivCh", SortOrder = 1 };
            var privL = new Lesson { Id = Guid.NewGuid(), ChapterId = privCh.Id, Title = "PrivL", Slug = $"privl-{Guid.NewGuid():N}", SortOrder = 1, PlainText = "Secret", IsPublished = true };

            db.Courses.Add(course);
            db.Chapters.AddRange(ch, privCh);
            db.Lessons.AddRange(l1, l2, l3, privL);
            await db.SaveChangesAsync();

            courseId = course.Id;
            lesson1Id = l1.Id;
            lesson2Id = l2.Id;
            draftLessonId = l3.Id;
            privateLessonId = privL.Id;
        }

        // Initial resume-state falls back to first uncompleted published lesson (lesson1)
        var initResumeResp = await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{courseId}/resume-state", tokenLearner);
        Assert.Equal(HttpStatusCode.OK, initResumeResp.StatusCode);
        var initResume = await initResumeResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(lesson1Id, initResume.GetProperty("lastViewedLessonId").GetGuid());
        Assert.Equal(0, initResume.GetProperty("totalTimeSpentSeconds").GetInt32());
        Assert.Equal(2, initResume.GetProperty("lessonProgress").GetArrayLength());

        // Post telemetry to lesson2
        var tel1Resp = await SendWithBearerAsync(
            client,
            HttpMethod.Post,
            $"/api/v1/lessons/{lesson2Id}/telemetry",
            tokenLearner,
            new { timeSpentDeltaSeconds = 45, scrollPercent = 60, lastScrollOffset = 1200 });
        Assert.Equal(HttpStatusCode.OK, tel1Resp.StatusCode);
        var tel1 = await tel1Resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(lesson2Id, tel1.GetProperty("lessonId").GetGuid());
        Assert.Equal(45, tel1.GetProperty("timeSpentSeconds").GetInt32());
        Assert.Equal(60, tel1.GetProperty("scrollPercent").GetInt32());
        Assert.Equal(1200, tel1.GetProperty("lastScrollOffset").GetInt32());
        Assert.False(tel1.GetProperty("isCompleted").GetBoolean());

        // Second telemetry clamps delta to 300 and preserves max scrollPercent
        var tel2Resp = await SendWithBearerAsync(
            client,
            HttpMethod.Post,
            $"/api/v1/lessons/{lesson2Id}/telemetry",
            tokenLearner,
            new { timeSpentDeltaSeconds = 999, scrollPercent = 40, lastScrollOffset = 850 });
        Assert.Equal(HttpStatusCode.OK, tel2Resp.StatusCode);
        var tel2 = await tel2Resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(345, tel2.GetProperty("timeSpentSeconds").GetInt32());
        Assert.Equal(60, tel2.GetProperty("scrollPercent").GetInt32());
        Assert.Equal(850, tel2.GetProperty("lastScrollOffset").GetInt32());
        Assert.False(tel2.GetProperty("isCompleted").GetBoolean());

        // Resume state now points to lesson2 with aggregated stats
        var resumeResp = await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{courseId}/resume-state", tokenLearner);
        Assert.Equal(HttpStatusCode.OK, resumeResp.StatusCode);
        var resume = await resumeResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(lesson2Id, resume.GetProperty("lastViewedLessonId").GetGuid());
        Assert.Equal(850, resume.GetProperty("lastScrollOffset").GetInt32());
        Assert.Equal(60, resume.GetProperty("scrollPercent").GetInt32());
        Assert.Equal(345, resume.GetProperty("totalTimeSpentSeconds").GetInt32());

        // Security: Learner cannot post telemetry to draft lesson or private course lesson
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{draftLessonId}/telemetry", tokenLearner, new { timeSpentDeltaSeconds = 10 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{privateLessonId}/telemetry", tokenLearner, new { timeSpentDeltaSeconds = 10 })).StatusCode);
    }

    [Fact]
    public async Task ReorderCourse_OwnerCanReorderChaptersAndMoveLessons_NonOwnerReturns403()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (tokenOwner, ownerId, _) = await RegisterUserAsync(client, "reorder.owner", "Reorder Owner");
        var (tokenOther, _, _) = await RegisterUserAsync(client, "reorder.other", "Other User");

        Guid courseId, ch1Id, ch2Id, l1Id, l2Id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = ownerId,
                Title = "Reorder Course",
                Slug = $"reorder-course-{Guid.NewGuid():N}",
                Visibility = "public",
                IsPublished = true
            };
            var ch1 = new Chapter { Id = Guid.NewGuid(), CourseId = course.Id, Title = "Chapter 1", SortOrder = 1 };
            var ch2 = new Chapter { Id = Guid.NewGuid(), CourseId = course.Id, Title = "Chapter 2", SortOrder = 2 };
            var l1 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch1.Id, Title = "Lesson 1", Slug = $"rl1-{Guid.NewGuid():N}", SortOrder = 1, PlainText = "1", IsPublished = true };
            var l2 = new Lesson { Id = Guid.NewGuid(), ChapterId = ch2.Id, Title = "Lesson 2", Slug = $"rl2-{Guid.NewGuid():N}", SortOrder = 1, PlainText = "2", IsPublished = true };

            db.Courses.Add(course);
            db.Chapters.AddRange(ch1, ch2);
            db.Lessons.AddRange(l1, l2);
            await db.SaveChangesAsync();

            courseId = course.Id;
            ch1Id = ch1.Id;
            ch2Id = ch2.Id;
            l1Id = l1.Id;
            l2Id = l2.Id;
        }

        var reorderPayload = new
        {
            chapters = new[]
            {
                new { chapterId = ch2Id, sortOrder = 1 },
                new { chapterId = ch1Id, sortOrder = 2 }
            },
            lessons = new[]
            {
                new { lessonId = l2Id, chapterId = (Guid?)ch2Id, sortOrder = 1 },
                new { lessonId = l1Id, chapterId = (Guid?)ch2Id, sortOrder = 2 }
            }
        };

        // Non-owner -> 403
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Put, $"/api/v1/courses/{courseId}/reorder", tokenOther, reorderPayload)).StatusCode);

        // Owner -> 200 and persisted
        var okResp = await SendWithBearerAsync(client, HttpMethod.Put, $"/api/v1/courses/{courseId}/reorder", tokenOwner, reorderPayload);
        Assert.Equal(HttpStatusCode.OK, okResp.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var updatedCh1 = await db.Chapters.AsNoTracking().FirstAsync(c => c.Id == ch1Id);
            var updatedCh2 = await db.Chapters.AsNoTracking().FirstAsync(c => c.Id == ch2Id);
            var updatedL1 = await db.Lessons.AsNoTracking().FirstAsync(l => l.Id == l1Id);
            var updatedL2 = await db.Lessons.AsNoTracking().FirstAsync(l => l.Id == l2Id);

            Assert.Equal(2, updatedCh1.SortOrder);
            Assert.Equal(1, updatedCh2.SortOrder);
            Assert.Equal(ch2Id, updatedL1.ChapterId);
            Assert.Equal(2, updatedL1.SortOrder);
            Assert.Equal(ch2Id, updatedL2.ChapterId);
            Assert.Equal(1, updatedL2.SortOrder);
        }
    }

    [Fact]
    public async Task CourseReviews_ValidatesRating_UpsertsPerUser_AndComputesAverage()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (_, ownerId, ownerPersonalCourseId) = await RegisterUserAsync(client, "rev.owner", "Review Owner");
        var (tokenB, _, _) = await RegisterUserAsync(client, "rev.b", "Nguyễn Bình");
        var (tokenC, _, _) = await RegisterUserAsync(client, "rev.c", "Trần Cúc");

        Guid publicCourseId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = ownerId,
                Title = "Reviewed Public Course",
                Slug = $"rev-course-{Guid.NewGuid():N}",
                Visibility = "public",
                IsPublished = true
            };
            db.Courses.Add(course);
            await db.SaveChangesAsync();
            publicCourseId = course.Id;
        }

        // Invalid ratings -> 400
        Assert.Equal(HttpStatusCode.BadRequest, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{publicCourseId}/reviews", tokenB, new { rating = 0, comment = "Bad" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{publicCourseId}/reviews", tokenB, new { rating = 6, comment = "Bad" })).StatusCode);

        // Private course -> 403
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{ownerPersonalCourseId}/reviews", tokenB, new { rating = 5, comment = "Hi" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{ownerPersonalCourseId}/reviews", tokenB)).StatusCode);

        // User B & User C post reviews
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{publicCourseId}/reviews", tokenB, new { rating = 5, comment = "Rất hay!" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{publicCourseId}/reviews", tokenC, new { rating = 3, comment = "Khá ổn." })).StatusCode);

        var getResp1 = await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{publicCourseId}/reviews", tokenB);
        Assert.Equal(HttpStatusCode.OK, getResp1.StatusCode);
        var revJson1 = await getResp1.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, revJson1.GetProperty("totalReviews").GetInt32());
        Assert.Equal(4.0, revJson1.GetProperty("averageRating").GetDouble(), 2);

        // User B updates existing review (upsert) -> still 2 reviews, average becomes 3.5
        Assert.Equal(HttpStatusCode.OK, (await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/courses/{publicCourseId}/reviews", tokenB, new { rating = 4, comment = "Cập nhật đánh giá." })).StatusCode);
        var revJson2 = await (await SendWithBearerAsync(client, HttpMethod.Get, $"/api/v1/courses/{publicCourseId}/reviews", tokenC)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, revJson2.GetProperty("totalReviews").GetInt32());
        Assert.Equal(3.5, revJson2.GetProperty("averageRating").GetDouble(), 2);
    }

    [Fact]
    public async Task AutoGenerateFsrsCards_ExtractsConceptsAndDeduplicates_AndPublicSeededQuizzesCloneRubricFeedback()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var (tokenA, userAId, _) = await RegisterUserAsync(client, "fsrs.gen", "Card Gen User");

        Guid lessonId;
        Guid seededQuizSourceLessonId;
        const string expectedRubricJson = "{\"explanation\":\"Đáp án B chính xác vì TLB giảm thời gian tra cứu bảng trang.\"}";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

            // Create a rich markdown lesson in a public course
            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = SeedDataInitializer.DefaultUserId,
                Title = "Khóa Public Seeded Non-Doc",
                Slug = $"seeded-public-quiz-{Guid.NewGuid():N}",
                Visibility = "public",
                IsPublished = true
            };
            var ch = new Chapter { Id = Guid.NewGuid(), CourseId = course.Id, Title = "Chương 1", SortOrder = 1 };
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                ChapterId = ch.Id,
                Title = "Kiến trúc Bộ nhớ đệm CPU & TLB",
                Slug = $"cache-tlb-{Guid.NewGuid():N}",
                SortOrder = 1,
                IsPublished = true,
                ContentMarkdown = """
                    # Kiến trúc Bộ nhớ đệm CPU & TLB

                    - **TLB (Translation Lookaside Buffer)**: Bộ nhớ đệm phần cứng tốc độ cao trong MMU ánh xạ trực tiếp trang ảo sang khung trang vật lý trong O(1).
                    - **Page Fault**: Sự kiện ngắt phần cứng xảy ra khi tiến trình truy cập một trang ảo chưa có mặt trong RAM vật lý.
                    - **Locality of Reference**: Nguyên lý tính cục bộ theo không gian và thời gian giúp bộ nhớ đệm đạt tỷ lệ trúng (Hit Ratio) trên 95%.

                    | Thuật ngữ | Định nghĩa ngắn gọn |
                    | --- | --- |
                    | MMU | Khối quản lý bộ nhớ phần cứng thực hiện dịch địa chỉ logic sang vật lý |
                    | Working Set | Tập các trang đang được tiến trình sử dụng tích cực trong cửa sổ thời gian gần nhất |

                    ## Bảng trang đa cấp
                    Hệ điều hành Linux x86-64 dùng bảng trang 4 tầng để tránh cấp phát vùng nhớ chỉ mục khổng lồ cho không gian 64-bit.
                    """,
                PlainText = "TLB, Page Fault, Locality of Reference, MMU, Working Set và Bảng trang đa cấp."
            };
            db.Courses.Add(course);
            db.Chapters.Add(ch);
            db.Lessons.Add(lesson);

            // Seed a DefaultUserId quiz on this non-document-course public course with AiRubricFeedbackJson
            var srcQuiz = new PracticeQuiz
            {
                Id = Guid.NewGuid(),
                UserId = SeedDataInitializer.DefaultUserId,
                LessonId = lesson.Id,
                Title = "Quiz Public Seeded Có Giải Thích",
                TotalItems = 1,
                CorrectItems = 0,
                Status = "in_progress"
            };
            db.PracticeQuizzes.Add(srcQuiz);
            db.PracticeQuizItems.Add(new PracticeQuizItem
            {
                Id = Guid.NewGuid(),
                QuizId = srcQuiz.Id,
                ItemType = "multiple_choice",
                PromptAstJson = "{\"question\":\"TLB viết tắt của gì?\"}",
                ChoicesJson = "[\"A. Total Load Bus\",\"B. Translation Lookaside Buffer\"]",
                ClozeSolution = "B",
                AiRubricFeedbackJson = expectedRubricJson
            });

            await db.SaveChangesAsync();
            lessonId = lesson.Id;
            seededQuizSourceLessonId = lesson.Id;
        }

        // 1. Auto-generate FSRS cards -> 3..5 ai_generated cards
        var gen1Resp = await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{lessonId}/auto-generate-fsrs-cards", tokenA);
        Assert.Equal(HttpStatusCode.OK, gen1Resp.StatusCode);
        var gen1 = await gen1Resp.Content.ReadFromJsonAsync<JsonElement>();
        var createdCount1 = gen1.GetProperty("createdCount").GetInt32();
        var totalCards1 = gen1.GetProperty("totalCards").GetInt32();
        Assert.InRange(createdCount1, 3, 5);
        Assert.Equal(createdCount1, totalCards1);
        Assert.All(gen1.GetProperty("cards").EnumerateArray(), c =>
        {
            Assert.Equal("ai_generated", c.GetProperty("cardType").GetString());
            Assert.Equal(lessonId, c.GetProperty("lessonId").GetGuid());
        });

        // 2. Calling again deduplicates -> creates 0 new cards
        var gen2Resp = await SendWithBearerAsync(client, HttpMethod.Post, $"/api/v1/lessons/{lessonId}/auto-generate-fsrs-cards", tokenA);
        Assert.Equal(HttpStatusCode.OK, gen2Resp.StatusCode);
        var gen2 = await gen2Resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, gen2.GetProperty("createdCount").GetInt32());
        Assert.Equal(totalCards1, gen2.GetProperty("totalCards").GetInt32());

        // 3. New user calling GET /api/v1/practice-quizzes gets cloned quiz for non-document-course public course with AiRubricFeedbackJson preserved
        var quizzesResp = await SendWithBearerAsync(client, HttpMethod.Get, "/api/v1/practice-quizzes", tokenA);
        Assert.Equal(HttpStatusCode.OK, quizzesResp.StatusCode);
        var quizzesArr = await quizzesResp.Content.ReadFromJsonAsync<JsonElement>();
        var matchedQuiz = quizzesArr.EnumerateArray()
            .FirstOrDefault(q => q.TryGetProperty("lessonId", out var lid) &&
                                 lid.ValueKind == JsonValueKind.String &&
                                 lid.GetGuid() == seededQuizSourceLessonId);
        Assert.Equal(JsonValueKind.Object, matchedQuiz.ValueKind);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var userQuiz = await db.PracticeQuizzes.AsNoTracking()
                .FirstOrDefaultAsync(q => q.UserId == userAId && q.LessonId == seededQuizSourceLessonId);
            Assert.NotNull(userQuiz);
            var clonedItem = await db.PracticeQuizItems.AsNoTracking()
                .FirstOrDefaultAsync(i => i.QuizId == userQuiz!.Id);
            Assert.NotNull(clonedItem);
            Assert.Equal(expectedRubricJson, clonedItem!.AiRubricFeedbackJson);
        }
    }

    [Fact]
    public async Task SqliteSchemaMigration_UpgradesLegacyUserLessonProgressAndCreatesCourseReviews()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        // Create legacy tables without new telemetry columns and without course_reviews
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE lessons (id TEXT NOT NULL PRIMARY KEY, title TEXT NOT NULL);
                CREATE TABLE users (id TEXT NOT NULL PRIMARY KEY, email TEXT NOT NULL);
                CREATE TABLE user_lesson_progress (
                    id TEXT NOT NULL PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    lesson_id TEXT NOT NULL,
                    is_completed INTEGER NOT NULL,
                    completed_at TEXT,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<SwmsDbContext>().UseSqlite(conn).Options;
        await using var db = new SwmsDbContext(options);

        await SwmsEndpoints.EnsureSqliteSchemaUpdatedAsync(db);

        var progressCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info('user_lesson_progress');";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) progressCols.Add(reader.GetString(1));
        }

        Assert.Contains("last_viewed_at", progressCols);
        Assert.Contains("time_spent_seconds", progressCols);
        Assert.Contains("scroll_percent", progressCols);
        Assert.Contains("last_scroll_offset", progressCols);

        var reviewCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info('course_reviews');";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) reviewCols.Add(reader.GetString(1));
        }

        Assert.Contains("course_id", reviewCols);
        Assert.Contains("user_id", reviewCols);
        Assert.Contains("rating", reviewCols);
        Assert.Contains("comment", reviewCols);
    }
}
