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

public class PracticeEngineTests
{
    private static WebApplicationFactory<Program> CreateIsolatedFactory()
    {
        var dbName = $"SwmsPracticeEngineTestDb_{Guid.NewGuid():N}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
            });
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
    public async Task Test1_GetActivities_ReturnsSeededEnglishCourseActivities()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // Query activities for seeded English course
        var url = $"/api/v1/practice/activities?courseId={SeedDataInitializer.EnglishCourseId}&lessonId={SeedDataInitializer.EnglishLesson11Id}";
        var res = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var activities = await res.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(activities);
        Assert.True(activities.Count >= 4, $"Expected at least 4 activities for Lesson 1.1, found {activities.Count}");

        var types = activities.Select(a => a.GetProperty("type").GetString()).ToList();
        Assert.Contains("shadowing", types);
        Assert.Contains("dictation", types);
        Assert.Contains("pronunciation", types);
        Assert.Contains("word_map", types);

        var wordMap = activities.First(a => a.GetProperty("type").GetString() == "word_map");
        var configJson = wordMap.GetProperty("configJson").GetString();
        Assert.False(string.IsNullOrWhiteSpace(configJson));
        Assert.Contains("centralWord", configJson);
        Assert.Contains("collocations", configJson);
    }

    [Fact]
    public async Task Test2_RecordAttempt_SavesToDb_And_ValidatesScoreBounds()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Invalid Score: negative -> 400
        var invalidNegative = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "shadowing",
            ActivityKey: "test-shadow-key",
            ActivityTitle: "Shadowing Practice Test",
            Score: -5,
            MaxScore: 100
        );
        var resNeg = await client.PostAsJsonAsync("/api/v1/practice/attempts", invalidNegative);
        Assert.Equal(HttpStatusCode.BadRequest, resNeg.StatusCode);

        // 2. Invalid Score: over max -> 400
        var invalidOver = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "shadowing",
            ActivityKey: "test-shadow-key",
            ActivityTitle: "Shadowing Practice Test",
            Score: 105,
            MaxScore: 100
        );
        var resOver = await client.PostAsJsonAsync("/api/v1/practice/attempts", invalidOver);
        Assert.Equal(HttpStatusCode.BadRequest, resOver.StatusCode);

        // 3. Valid Attempt -> 200 OK
        var validAttempt = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "dictation",
            ActivityKey: "lesson11-dictation-test",
            ActivityTitle: "Dictation Test 1",
            Score: 88.5,
            MaxScore: 100,
            IsPassed: true,
            DetailJson: "{\"correctWords\":10,\"totalWords\":11}",
            DurationSeconds: 45
        );
        var resValid = await client.PostAsJsonAsync("/api/v1/practice/attempts", validAttempt);
        Assert.Equal(HttpStatusCode.OK, resValid.StatusCode);

        var savedJson = await resValid.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(88.5, savedJson.GetProperty("score").GetDouble());
        Assert.True(savedJson.GetProperty("isPassed").GetBoolean());
        Assert.Equal("dictation", savedJson.GetProperty("activityType").GetString());
    }

    [Fact]
    public async Task Test3_GetAttemptsAndStats_AccuratelyReflectsUserPracticeHistory()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // Save 2 attempts: one passed (85), one failed (55)
        var attempt1 = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "shadowing",
            ActivityKey: "key-shadow-1",
            ActivityTitle: "Shadowing Lesson 1.1",
            Score: 85,
            MaxScore: 100,
            IsPassed: true
        );
        var res1 = await client.PostAsJsonAsync("/api/v1/practice/attempts", attempt1);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);

        var attempt2 = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "dictation",
            ActivityKey: "key-dictation-1",
            ActivityTitle: "Dictation Lesson 1.1",
            Score: 55,
            MaxScore: 100,
            IsPassed: false
        );
        var res2 = await client.PostAsJsonAsync("/api/v1/practice/attempts", attempt2);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);

        // Fetch user attempts list
        var listRes = await client.GetAsync($"/api/v1/practice/attempts?courseId={SeedDataInitializer.EnglishCourseId}");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        var attemptsList = await listRes.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(attemptsList);
        Assert.True(attemptsList.Count >= 2);

        // Fetch stats
        var statsRes = await client.GetAsync("/api/v1/practice/stats");
        Assert.Equal(HttpStatusCode.OK, statsRes.StatusCode);
        var stats = await statsRes.Content.ReadFromJsonAsync<JsonElement>();

        var total = stats.GetProperty("totalAttempts").GetInt32();
        var avg = stats.GetProperty("averageScore").GetDouble();
        var passed = stats.GetProperty("passedCount").GetInt32();
        var needsReview = stats.GetProperty("needsReview").EnumerateArray().ToList();

        Assert.True(total >= 2);
        Assert.True(avg > 0);
        Assert.True(passed >= 1);

        // Dictation failed (score 55 < 70 and isPassed=false) -> must be in needsReview
        var failedReviewItem = needsReview.FirstOrDefault(nr => nr.GetProperty("activityKey").GetString() == "key-dictation-1");
        Assert.True(failedReviewItem.ValueKind != JsonValueKind.Undefined, "Failed activity should appear in needsReview");
    }

    [Fact]
    public async Task Test4_MultiTenantIsolation_UserBCannotReadOrRecordToUserAPrivateCourse()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Register User A
        var regAResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"user.a.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "User A"));
        Assert.Equal(HttpStatusCode.OK, regAResp.StatusCode);
        var regAJson = await regAResp.Content.ReadFromJsonAsync<JsonElement>();
        var tokenA = regAJson.GetProperty("accessToken").GetString()!;
        var userAId = regAJson.GetProperty("user").GetProperty("id").GetGuid();

        // 2. Register User B
        var regBResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"user.b.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "User B"));
        Assert.Equal(HttpStatusCode.OK, regBResp.StatusCode);
        var regBJson = await regBResp.Content.ReadFromJsonAsync<JsonElement>();
        var tokenB = regBJson.GetProperty("accessToken").GetString()!;
        var userBId = regBJson.GetProperty("user").GetProperty("id").GetGuid();

        using var clientA = factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        using var clientB = factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // 3. Create a private course owned by User A directly in DB
        var courseAId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            db.Courses.Add(new Course
            {
                Id = courseAId,
                InstructorId = userAId,
                Title = "Khóa học riêng tư của User A",
                Slug = $"priv-course-{courseAId:N}",
                Description = "Dữ liệu riêng tư của User A",
                Visibility = "private",
                IsPublished = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 4. User A records a practice attempt in their course
        var attemptA = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: courseAId,
            LessonId: null,
            ActivityType: "shadowing",
            ActivityKey: "priv-shadow-key",
            ActivityTitle: "Shadowing in private course",
            Score: 92,
            MaxScore: 100,
            IsPassed: true
        );
        var recordAResp = await clientA.PostAsJsonAsync("/api/v1/practice/attempts", attemptA);
        Assert.Equal(HttpStatusCode.OK, recordAResp.StatusCode);

        // 5. User B attempts to read attempts of User A's private course -> 403 Forbidden
        var readBResp = await clientB.GetAsync($"/api/v1/practice/attempts?courseId={courseAId}");
        Assert.Equal(HttpStatusCode.Forbidden, readBResp.StatusCode);

        // 6. User B attempts to record an attempt into User A's private course -> 403 Forbidden
        var attemptB = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: courseAId,
            LessonId: null,
            ActivityType: "shadowing",
            ActivityKey: "user-b-unauthorized",
            ActivityTitle: "User B attempt",
            Score: 80,
            MaxScore: 100,
            IsPassed: true
        );
        var recordBResp = await clientB.PostAsJsonAsync("/api/v1/practice/attempts", attemptB);
        Assert.Equal(HttpStatusCode.Forbidden, recordBResp.StatusCode);

        // 7. User B calls GET /api/v1/practice/attempts (without courseId filter) -> User A's attempt must NOT appear
        var generalBResp = await clientB.GetAsync("/api/v1/practice/attempts");
        Assert.Equal(HttpStatusCode.OK, generalBResp.StatusCode);
        var generalBList = await generalBResp.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(generalBList);
        Assert.DoesNotContain(generalBList, item => item.GetProperty("activityKey").GetString() == "priv-shadow-key");
    }

    [Fact]
    public async Task Test5_GetActivities_ReturnsSeededInvestingCourseActivities()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var url = $"/api/v1/practice/activities?courseId={SeedDataInitializer.InvestingCourseId}&lessonId={SeedDataInitializer.InvestingLesson11Id}";
        var res = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var activities = await res.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(activities);
        Assert.True(activities.Count >= 2, $"Expected at least 2 activities, found {activities.Count}");

        var types = activities.Select(a => a.GetProperty("type").GetString()).ToList();
        Assert.Contains("compound_calculator", types);
        Assert.Contains("behavioral_quiz", types);

        var compoundAct = activities.First(a => a.GetProperty("type").GetString() == "compound_calculator");
        var cfg = compoundAct.GetProperty("configJson").GetString();
        Assert.Contains("initialPrincipal", cfg);
        Assert.Contains("monthlyDca", cfg);
    }

    [Fact]
    public async Task Test6_GetActivities_ReturnsSeededCodingCourseActivities()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var url = $"/api/v1/practice/activities?courseId={SeedDataInitializer.CodingCourseId}&lessonId={SeedDataInitializer.CodingLesson11Id}";
        var res = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var activities = await res.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(activities);
        Assert.True(activities.Count >= 2, $"Expected at least 2 activities, found {activities.Count}");

        var types = activities.Select(a => a.GetProperty("type").GetString()).ToList();
        Assert.Contains("js_sandbox", types);
        Assert.Contains("parsons", types);

        var jsAct = activities.First(a => a.GetProperty("type").GetString() == "js_sandbox");
        var cfg = jsAct.GetProperty("configJson").GetString();
        Assert.Contains("starterCode", cfg);
        Assert.Contains("testCases", cfg);
    }

    [Fact]
    public async Task Test7_GetActivities_ReturnsSeededPsychologyCourseActivities()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var url = $"/api/v1/practice/activities?courseId={SeedDataInitializer.PsychologyCourseId}&lessonId={SeedDataInitializer.PsychologyLesson11Id}";
        var res = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var activities = await res.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(activities);
        Assert.True(activities.Count >= 2, $"Expected at least 2 activities, found {activities.Count}");

        var types = activities.Select(a => a.GetProperty("type").GetString()).ToList();
        Assert.Contains("stroop_test", types);
        Assert.Contains("psych_scenario", types);

        var stroopAct = activities.First(a => a.GetProperty("type").GetString() == "stroop_test");
        var cfg = stroopAct.GetProperty("configJson").GetString();
        Assert.Contains("trialsCount", cfg);
        Assert.Contains("words", cfg);
    }

    [Fact]
    public async Task Test8_RealSqliteProvider_GetAllPracticeEndpoints_DoesNotThrowDateTimeOffsetOrderException()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        // 1. Create a practice attempt on real SQLite with CreatedAt DateTimeOffset
        var attempt = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "dictation",
            ActivityKey: "real-sqlite-test-key",
            ActivityTitle: "Real SQLite Test Dictation",
            Score: 95.0,
            MaxScore: 100,
            IsPassed: true,
            DetailJson: "{\"words\":10}",
            DurationSeconds: 30
        );
        var postRes = await client.PostAsJsonAsync("/api/v1/practice/attempts", attempt);
        Assert.Equal(HttpStatusCode.OK, postRes.StatusCode);

        // 2. Call GET /api/v1/practice/activities on real SQLite (must not throw SQLite DateTimeOffset OrderBy exception)
        var actRes = await client.GetAsync($"/api/v1/practice/activities?courseId={SeedDataInitializer.EnglishCourseId}");
        Assert.Equal(HttpStatusCode.OK, actRes.StatusCode);
        var activities = await actRes.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(activities);
        Assert.NotEmpty(activities);

        // 3. Call GET /api/v1/practice/attempts on real SQLite (must not throw SQLite DateTimeOffset OrderBy exception)
        var attRes = await client.GetAsync($"/api/v1/practice/attempts?courseId={SeedDataInitializer.EnglishCourseId}&limit=10");
        Assert.Equal(HttpStatusCode.OK, attRes.StatusCode);
        var attemptsList = await attRes.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(attemptsList);
        Assert.NotEmpty(attemptsList);
        Assert.Contains(attemptsList, a => a.GetProperty("activityKey").GetString() == "real-sqlite-test-key");

        // 4. Call GET /api/v1/practice/stats on real SQLite (must not throw SQLite DateTimeOffset OrderBy exception)
        var statsRes = await client.GetAsync("/api/v1/practice/stats");
        Assert.Equal(HttpStatusCode.OK, statsRes.StatusCode);
        var stats = await statsRes.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(stats.GetProperty("totalAttempts").GetInt32() >= 1);
    }

    [Fact]
    public async Task Test9_AiEvaluatePractice_ReturnsOfflineKeywordGradingWhenProxyUnavailable()
    {
        var dbName = $"SwmsAiEvalOfflineTestDb_{Guid.NewGuid():N}";
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
                // Force unreachable loopback port so test deterministically verifies offline keyword fallback
                builder.UseSetting("AiProxy:BaseUrl", "http://127.0.0.1:59997");
                builder.UseSetting("AiProxy:TimeoutSeconds", "2");
            });
        using var client = factory.CreateClient();

        // 1. Empty userResponse -> 400 BadRequest
        var emptyReq = new AiEvaluatePracticeRequest(
            Mode: "roleplay",
            Lang: "en-US",
            Prompt: "Hi! Is this seat taken?",
            UserResponse: "   ",
            ExpectedKeywords: new List<string> { "go ahead", "pleasure" },
            SampleAnswer: "No, please go ahead! It is a pleasure to meet you."
        );
        var emptyRes = await client.PostAsJsonAsync("/api/v1/ai/evaluate-practice", emptyReq);
        Assert.Equal(HttpStatusCode.BadRequest, emptyRes.StatusCode);

        // 2. Strong keyword answer with offline proxy -> deterministic offline grading + honest label
        var goodReq = new AiEvaluatePracticeRequest(
            Mode: "roleplay",
            Lang: "en-US",
            Prompt: "Hi! Is this seat taken?",
            UserResponse: "No, please go ahead! It is a pleasure to meet you, my name is Mai.",
            ExpectedKeywords: new List<string> { "go ahead", "pleasure", "meet", "name" },
            SampleAnswer: "No, please go ahead! It is a pleasure to meet you.",
            ContextTitle: "Lesson 1.1 Small Talk"
        );
        var goodRes = await client.PostAsJsonAsync("/api/v1/ai/evaluate-practice", goodReq);
        Assert.Equal(HttpStatusCode.OK, goodRes.StatusCode);
        var goodJson = await goodRes.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(goodJson.GetProperty("usedLiveEndpoint").GetBoolean());
        Assert.Equal("keyword_offline", goodJson.GetProperty("gradingMode").GetString());
        Assert.Equal("Chấm theo từ khóa (offline)", goodJson.GetProperty("gradingLabelVi").GetString());
        Assert.True(goodJson.GetProperty("score").GetInt32() >= 80, $"Expected deterministic score >= 80, got {goodJson.GetProperty("score").GetInt32()}");
        Assert.Contains("Chấm theo từ khóa", goodJson.GetProperty("feedbackVi").GetString());

        // 3. Weak answer missing keywords -> significantly lower deterministic score
        var weakReq = new AiEvaluatePracticeRequest(
            Mode: "roleplay",
            Lang: "en-US",
            Prompt: "Hi! Is this seat taken?",
            UserResponse: "Okay",
            ExpectedKeywords: new List<string> { "go ahead", "pleasure", "meet", "name" },
            SampleAnswer: "No, please go ahead! It is a pleasure to meet you."
        );
        var weakRes = await client.PostAsJsonAsync("/api/v1/ai/evaluate-practice", weakReq);
        Assert.Equal(HttpStatusCode.OK, weakRes.StatusCode);
        var weakJson = await weakRes.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(weakJson.GetProperty("usedLiveEndpoint").GetBoolean());
        Assert.Equal("keyword_offline", weakJson.GetProperty("gradingMode").GetString());
        Assert.True(weakJson.GetProperty("score").GetInt32() < goodJson.GetProperty("score").GetInt32());
    }

    [Fact]
    public async Task Test10_EnsureEnglishCourseSeeded_IsIdempotent_And_UpgradesLegacyDbWithoutDuplicatingOldActivities()
    {
        await using var factory = CreateIsolatedFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

        // 1. Verify initial seed includes both legacy 4 types and new schema types for Lesson 1.1
        var initialL11 = await db.PracticeActivities
            .Where(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId && pa.LessonId == SeedDataInitializer.EnglishLesson11Id)
            .ToListAsync();
        var initialTypes = initialL11.Select(a => a.Type).ToHashSet();
        Assert.Contains("shadowing", initialTypes);
        Assert.Contains("dictation", initialTypes);
        Assert.Contains("pronunciation", initialTypes);
        Assert.Contains("word_map", initialTypes);
        Assert.Contains("cloze", initialTypes);
        Assert.Contains("translate", initialTypes);
        Assert.Contains("grammar_fix", initialTypes);
        Assert.Contains("roleplay", initialTypes);

        var totalBefore = await db.PracticeActivities.CountAsync(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId);

        // 2. Re-running seeder twice must NOT create any duplicate activities
        await SeedDataInitializer.EnsureEnglishCommunicationCourseSeededAsync(db);
        await SeedDataInitializer.EnsureEnglishCommunicationCourseSeededAsync(db);

        var totalAfter = await db.PracticeActivities.CountAsync(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId);
        Assert.Equal(totalBefore, totalAfter);

        // 3. Simulate legacy DB (only OrderIndex 1..4 with old random GUIDs) and verify upgrade preserves old IDs & adds 5..10
        var extendedRows = await db.PracticeActivities
            .Where(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId && pa.LessonId == SeedDataInitializer.EnglishLesson11Id && pa.OrderIndex >= 5)
            .ToListAsync();
        db.PracticeActivities.RemoveRange(extendedRows);
        await db.SaveChangesAsync();

        var legacyCoreIds = await db.PracticeActivities
            .Where(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId && pa.LessonId == SeedDataInitializer.EnglishLesson11Id && pa.OrderIndex <= 4)
            .Select(pa => pa.Id)
            .ToListAsync();
        Assert.Equal(4, legacyCoreIds.Count);

        await SeedDataInitializer.EnsureEnglishCommunicationCourseSeededAsync(db);

        var upgradedL11 = await db.PracticeActivities
            .Where(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId && pa.LessonId == SeedDataInitializer.EnglishLesson11Id)
            .ToListAsync();
        Assert.Equal(initialL11.Count, upgradedL11.Count);
        foreach (var oldId in legacyCoreIds)
        {
            Assert.Contains(upgradedL11, a => a.Id == oldId);
        }
    }

    [Fact]
    public async Task Test11_CreateFlashcard_WithDeduplicateByFront_PreventsDuplicateCardsCaseInsensitive()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var firstReq = new CreateFlashcardRequest(
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            DocumentId: null,
            AnnotationId: null,
            CardType: "standard",
            SourceTitleSnapshot: "Lesson 1.1",
            SourceQuoteSnapshot: "relationships",
            FrontText: "  Relationships  ",
            BackText: "Các mối quan hệ",
            TagNames: new List<string> { "tieng-anh", "loi-sai-luyen-tap" },
            DeduplicateByFront: true
        );
        var res1 = await client.PostAsJsonAsync("/api/v1/flashcards", firstReq);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
        var card1 = await res1.Content.ReadFromJsonAsync<JsonElement>();
        var id1 = card1.GetProperty("id").GetGuid();

        var dupReq = firstReq with { FrontText = "relationships" };
        var res2 = await client.PostAsJsonAsync("/api/v1/flashcards", dupReq);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        var card2 = await res2.Content.ReadFromJsonAsync<JsonElement>();
        var id2 = card2.GetProperty("id").GetGuid();

        Assert.Equal(id1, id2);
        Assert.True(res2.Headers.TryGetValues("X-Swms-Flashcard-Deduplicated", out var dedupHeader));
        Assert.Contains("true", dedupHeader);
    }

    [Fact]
    public async Task Test12_AllSixEnglishLessons_HaveAllEightActivityTypes_AndAreIdempotent()
    {
        await using var factory = CreateIsolatedFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

        var lessonIds = new[]
        {
            SeedDataInitializer.EnglishLesson11Id,
            SeedDataInitializer.EnglishLesson12Id,
            SeedDataInitializer.EnglishLesson21Id,
            SeedDataInitializer.EnglishLesson22Id,
            SeedDataInitializer.EnglishLesson31Id,
            SeedDataInitializer.EnglishLesson32Id
        };

        var expectedTypes = new[]
        {
            "shadowing", "dictation", "pronunciation", "word_map",
            "cloze", "translate", "grammar_fix", "roleplay"
        };

        // 1. Verify all 6 lessons exist and contain the upgrade marker
        var lessons = await db.Lessons.Where(l => lessonIds.Contains(l.Id)).ToListAsync();
        Assert.Equal(6, lessons.Count);
        foreach (var lesson in lessons)
        {
            Assert.Contains("<!-- studxy-en-v2 -->", lesson.ContentMarkdown);
        }

        // 2. Verify each of the 6 lessons has all 8 activity types
        var allActivities = await db.PracticeActivities
            .Where(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId)
            .ToListAsync();

        foreach (var lId in lessonIds)
        {
            var actsForLesson = allActivities.Where(pa => pa.LessonId == lId).ToList();
            var typesForLesson = actsForLesson.Select(pa => pa.Type).ToHashSet();
            foreach (var expectedType in expectedTypes)
            {
                Assert.Contains(expectedType, typesForLesson);
            }
        }

        var totalActivitiesCount = allActivities.Count;

        // 3. User edits a lesson: simulate Studio editor update (CurrentVersion = 2 and custom Markdown)
        var userEditedLesson = lessons.First(l => l.Id == SeedDataInitializer.EnglishLesson12Id);
        userEditedLesson.CurrentVersion = 2;
        userEditedLesson.ContentMarkdown = "# Nội dung do người dùng tự biên soạn tại Studio\nTuần này chỉ tập trung vào mua sắm siêu thị.";
        await db.SaveChangesAsync();

        // 4. Re-run seeding multiple times
        await SeedDataInitializer.EnsureEnglishCommunicationCourseSeededAsync(db);
        await SeedDataInitializer.EnsureEnglishCommunicationCourseSeededAsync(db);

        // Verify activities are NOT duplicated
        var totalAfter = await db.PracticeActivities.CountAsync(pa => pa.CourseId == SeedDataInitializer.EnglishCourseId);
        Assert.Equal(totalActivitiesCount, totalAfter);

        // Verify user edited lesson was NOT overwritten!
        var reloadedLesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == SeedDataInitializer.EnglishLesson12Id);
        Assert.NotNull(reloadedLesson);
        Assert.Equal(2, reloadedLesson.CurrentVersion);
        Assert.Contains("Nội dung do người dùng tự biên soạn tại Studio", reloadedLesson.ContentMarkdown);
        Assert.DoesNotContain("<!-- studxy-en-v2 -->", reloadedLesson.ContentMarkdown);
    }

    [Fact]
    public async Task Test13_PracticeStats_NeedsReviewLifecycle_CourseSummaries_And_FsrsDueCount_ReflectRealDb()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        await using var factory = CreateRealSqliteFactory(conn);
        using var rootClient = factory.CreateClient();

        // 1. Register an isolated fresh user so counts are 100% deterministic
        var regResp = await rootClient.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"hub.user.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Hub Tester"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var token = regJson.GetProperty("accessToken").GetString()!;
        var userId = regJson.GetProperty("user").GetProperty("id").GetGuid();

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Seed flashcards (2 due now, 1 due next week, 1 archived due now) + resolve a real PracticeActivity
        Guid realDictationActivityId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var now = DateTimeOffset.UtcNow;

            var act = await db.PracticeActivities.FirstAsync(a =>
                a.CourseId == SeedDataInitializer.EnglishCourseId &&
                a.LessonId == SeedDataInitializer.EnglishLesson11Id &&
                a.Type == "dictation");
            realDictationActivityId = act.Id;

            db.Flashcards.AddRange(
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = SeedDataInitializer.EnglishLesson11Id,
                    FrontAstJson = "{\"text\":\"Due 1\"}",
                    BackAstJson = "{\"text\":\"Answer 1\"}",
                    DueAt = now.AddMinutes(-10),
                    IsArchived = false
                },
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = SeedDataInitializer.EnglishLesson11Id,
                    FrontAstJson = "{\"text\":\"Due 2\"}",
                    BackAstJson = "{\"text\":\"Answer 2\"}",
                    DueAt = now.AddMinutes(-1),
                    IsArchived = false
                },
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = SeedDataInitializer.EnglishLesson11Id,
                    FrontAstJson = "{\"text\":\"Future card\"}",
                    BackAstJson = "{\"text\":\"Not due yet\"}",
                    DueAt = now.AddDays(5),
                    IsArchived = false
                },
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = SeedDataInitializer.EnglishLesson11Id,
                    FrontAstJson = "{\"text\":\"Archived due\"}",
                    BackAstJson = "{\"text\":\"Ignored\"}",
                    DueAt = now.AddMinutes(-30),
                    IsArchived = true
                });
            await db.SaveChangesAsync();
        }

        // 3. Submit a FAILING attempt in Coding course -> must appear in needsReview with courseTitle & lessonTitle
        var failCodingReq = new RecordPracticeAttemptRequest(
            ActivityId: null,
            CourseId: SeedDataInitializer.CodingCourseId,
            LessonId: SeedDataInitializer.CodingLesson11Id,
            ActivityType: "js_sandbox",
            ActivityKey: "coding-fail-retry-key",
            ActivityTitle: "JS Sandbox Two Sum",
            Score: 40,
            MaxScore: 100,
            IsPassed: false
        );
        var failRes = await client.PostAsJsonAsync("/api/v1/practice/attempts", failCodingReq);
        Assert.Equal(HttpStatusCode.OK, failRes.StatusCode);

        var statsAfterFailRes = await client.GetAsync("/api/v1/practice/stats");
        Assert.Equal(HttpStatusCode.OK, statsAfterFailRes.StatusCode);
        var statsAfterFail = await statsAfterFailRes.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(2, statsAfterFail.GetProperty("fsrsDueCount").GetInt32());
        Assert.Equal(1, statsAfterFail.GetProperty("totalAttempts").GetInt32());

        var reviewAfterFail = statsAfterFail.GetProperty("needsReview").EnumerateArray().ToList();
        var codingItem = reviewAfterFail.FirstOrDefault(x => x.GetProperty("activityKey").GetString() == "coding-fail-retry-key");
        Assert.True(codingItem.ValueKind != JsonValueKind.Undefined, "Failed coding activity should appear in needsReview");
        Assert.False(string.IsNullOrWhiteSpace(codingItem.GetProperty("courseTitle").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(codingItem.GetProperty("lessonTitle").GetString()));
        Assert.Equal(1, codingItem.GetProperty("attemptCount").GetInt32());

        // 4. Retry the same activityKey and PASS + submit a passed attempt for a seeded English PracticeActivity
        await Task.Delay(20);
        var passCodingReq = failCodingReq with { Score = 90, IsPassed = true };
        var passCodingRes = await client.PostAsJsonAsync("/api/v1/practice/attempts", passCodingReq);
        Assert.Equal(HttpStatusCode.OK, passCodingRes.StatusCode);

        var passEnglishActReq = new RecordPracticeAttemptRequest(
            ActivityId: realDictationActivityId,
            CourseId: SeedDataInitializer.EnglishCourseId,
            LessonId: SeedDataInitializer.EnglishLesson11Id,
            ActivityType: "dictation",
            ActivityKey: realDictationActivityId.ToString(),
            ActivityTitle: "English Dictation 1.1",
            Score: 95,
            MaxScore: 100,
            IsPassed: true
        );
        var passEngRes = await client.PostAsJsonAsync("/api/v1/practice/attempts", passEnglishActReq);
        Assert.Equal(HttpStatusCode.OK, passEngRes.StatusCode);

        // 5. Verify needsReview no longer contains the passed item, and courseSummaries reflects real DB counts
        var statsAfterPassRes = await client.GetAsync("/api/v1/practice/stats");
        Assert.Equal(HttpStatusCode.OK, statsAfterPassRes.StatusCode);
        var statsAfterPass = await statsAfterPassRes.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(3, statsAfterPass.GetProperty("totalAttempts").GetInt32());
        Assert.Equal(2, statsAfterPass.GetProperty("passedCount").GetInt32());
        Assert.Equal(2, statsAfterPass.GetProperty("fsrsDueCount").GetInt32());

        var reviewAfterPass = statsAfterPass.GetProperty("needsReview").EnumerateArray().ToList();
        Assert.DoesNotContain(reviewAfterPass, x => x.GetProperty("activityKey").GetString() == "coding-fail-retry-key");

        var courseSummaries = statsAfterPass.GetProperty("courseSummaries").EnumerateArray().ToList();
        Assert.NotEmpty(courseSummaries);

        var englishSummary = courseSummaries.FirstOrDefault(c => c.GetProperty("courseId").GetGuid() == SeedDataInitializer.EnglishCourseId);
        Assert.True(englishSummary.ValueKind != JsonValueKind.Undefined);
        Assert.True(englishSummary.GetProperty("totalActivities").GetInt32() >= 8);
        Assert.Equal(1, englishSummary.GetProperty("passedActivities").GetInt32());
        Assert.Equal(95.0, englishSummary.GetProperty("averageScore").GetDouble());
        Assert.True(englishSummary.GetProperty("lastAttemptAt").ValueKind != JsonValueKind.Null);

        var engLessons = englishSummary.GetProperty("lessons").EnumerateArray().ToList();
        var l11 = engLessons.FirstOrDefault(l => l.GetProperty("lessonId").GetGuid() == SeedDataInitializer.EnglishLesson11Id);
        Assert.True(l11.ValueKind != JsonValueKind.Undefined);
        Assert.False(string.IsNullOrWhiteSpace(l11.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(l11.GetProperty("chapterTitle").GetString()));
        Assert.True(l11.GetProperty("activityCount").GetInt32() >= 8);
        Assert.Equal(1, l11.GetProperty("passedCount").GetInt32());
    }
}
