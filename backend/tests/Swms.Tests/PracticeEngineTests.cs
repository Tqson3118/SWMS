using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
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
}
