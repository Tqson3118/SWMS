using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Swms.Api.Domain.Entities;
using Swms.Api.Features;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.FSRS;
using Swms.Api.Infrastructure.Security;
using Xunit;

namespace Swms.Tests;

public class SecurityAndAuditRegressionTests
{
    private static WebApplicationFactory<Program> CreateIsolatedFactory(bool enforceStrictBearer = false)
    {
        var dbName = $"SwmsSecurityAuditDb_{Guid.NewGuid():N}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
                if (enforceStrictBearer)
                {
                    builder.UseSetting("Auth:EnforceStrictBearer", "true");
                }
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
    public async Task StrictAuth_RejectsUnauthenticatedAndTestHeader_AndHidesPasswordHashAndHealthInternals()
    {
        await using var factory = CreateIsolatedFactory(enforceStrictBearer: true);
        using var client = factory.CreateClient();

        // 1. Public /health hides internal databaseMode, aiEndpoint, aiModel
        var healthResp = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResp.StatusCode);
        var healthJson = await healthResp.Content.ReadAsStringAsync();
        Assert.DoesNotContain("databaseMode", healthJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("aiEndpoint", healthJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("aiModel", healthJson, StringComparison.OrdinalIgnoreCase);

        // 2. Public /courses/template-zip works without JWT
        var tplResp = await client.GetAsync("/api/v1/courses/template-zip");
        Assert.Equal(HttpStatusCode.OK, tplResp.StatusCode);

        // 3. Protected endpoints reject missing JWT and X-Test-User-Id in strict mode
        var meUnauth = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meUnauth.StatusCode);

        using var meTestHeaderReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meTestHeaderReq.Headers.Add("X-Test-User-Id", SeedDataInitializer.DefaultUserId.ToString());
        var meTestHeaderResp = await client.SendAsync(meTestHeaderReq);
        Assert.Equal(HttpStatusCode.Unauthorized, meTestHeaderResp.StatusCode);

        using var logoutTestHeaderReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutTestHeaderReq.Headers.Add("X-Test-User-Id", SeedDataInitializer.DefaultUserId.ToString());
        var logoutTestHeaderResp = await client.SendAsync(logoutTestHeaderReq);
        Assert.Equal(HttpStatusCode.Unauthorized, logoutTestHeaderResp.StatusCode);

        var coursesUnauth = await client.GetAsync("/api/v1/courses");
        Assert.Equal(HttpStatusCode.Unauthorized, coursesUnauth.StatusCode);

        // 4. Register + anonymous Login (new user & seeded son.tran) + GET /auth/me with Bearer JWT succeeds
        var regEmail = $"strict.{Guid.NewGuid():N}@studxy.vn";
        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(regEmail, "Studxy@2026", "Strict User"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var token = regJson.GetProperty("accessToken").GetString()!;

        var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(regEmail, "Studxy@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        var seededLoginResp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("son.tran@studxy.vn", "Studxy@2026"));
        Assert.Equal(HttpStatusCode.OK, seededLoginResp.StatusCode);

        using var meAuthReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meAuthReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meAuthResp = await client.SendAsync(meAuthReq);
        Assert.Equal(HttpStatusCode.OK, meAuthResp.StatusCode);
        var meBody = await meAuthResp.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", meBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_NullOrInvalidInput_Returns400_Not500()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var nullEmailResp = await client.PostAsJsonAsync("/api/v1/auth/register", new { email = (string?)null, password = "password123", fullName = "" });
        Assert.Equal(HttpStatusCode.BadRequest, nullEmailResp.StatusCode);

        var invalidEmailResp = await client.PostAsJsonAsync("/api/v1/auth/register", new { email = "not-an-email", password = "password123", fullName = "User" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidEmailResp.StatusCode);

        var shortPwdResp = await client.PostAsJsonAsync("/api/v1/auth/register", new { email = "valid@studxy.vn", password = "123", fullName = "User" });
        Assert.Equal(HttpStatusCode.BadRequest, shortPwdResp.StatusCode);
    }

    [Fact]
    public async Task PasswordHashUtility_UsesV2SaltedPbkdf2_AndUpgradesLegacyV1OnLogin()
    {
        var hash1 = PasswordHashUtility.HashPassword("Studxy@2026");
        var hash2 = PasswordHashUtility.HashPassword("Studxy@2026");
        Assert.StartsWith("pbkdf2-sha256:v2:210000:", hash1);
        Assert.NotEqual(hash1, hash2); // Random 16-byte salt
        Assert.True(PasswordHashUtility.VerifyPassword("Studxy@2026", hash1));
        Assert.False(PasswordHashUtility.VerifyPassword("WrongPassword", hash1));

        // Dual-verify legacy v1 hash & auto-upgrade on login
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var email = $"legacy.{Guid.NewGuid():N}@studxy.vn";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            // Legacy v1 hash for "Studxy@2026"
            var legacySalt = Encoding.UTF8.GetBytes("SWMS-v1-Static-PBKDF2-Salt-2026");
            var legacyBytes = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes("Studxy@2026"),
                legacySalt,
                10000,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                32);
            var legacyHash = Convert.ToHexString(legacyBytes).ToLowerInvariant();

            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = legacyHash,
                FullName = "Legacy User",
                Role = "student",
                TokenVersion = 1
            });
            await db.SaveChangesAsync();
        }

        var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "Studxy@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == email);
            Assert.StartsWith("pbkdf2-sha256:v2:210000:", user.PasswordHash);
            Assert.True(PasswordHashUtility.VerifyPassword("Studxy@2026", user.PasswordHash));
        }
    }

    [Fact]
    public async Task RefreshToken_RealSqlite_RotatesCleanlyWithout500()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var factory = CreateRealSqliteFactory(conn);
        using var client = factory.CreateClient();

        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"sqlite.rtr.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "SQLite RTR"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var refreshToken = regJson.GetProperty("refreshToken").GetString()!;

        var refreshResp = await client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(refreshToken));
        Assert.Equal(HttpStatusCode.OK, refreshResp.StatusCode);
        var refreshJson = await refreshResp.Content.ReadFromJsonAsync<JsonElement>();
        var newRefresh = refreshJson.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(newRefresh));
        Assert.NotEqual(refreshToken, newRefresh);
    }

    [Fact]
    public async Task AiProxy_SsrfAndApiKeyExfiltration_BlockedForPrivateAndLoopbackUrls()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        bool receivedConnection = false;

        var acceptTask = Task.Run(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                using var tcp = await listener.AcceptTcpClientAsync(cts.Token);
                receivedConnection = true;
            }
            catch { }
        });

        try
        {
            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AiProxy:BaseUrl"] = "http://127.0.0.1:8045",
                    ["AiProxy:ApiKey"] = "sk-server-secret-key",
                    ["AiProxy:Model"] = "gemini-3.8-flash",
                    ["AiProxy:TimeoutSeconds"] = "2"
                })
                .Build();

            using var httpClient = new HttpClient();
            var client = new GeminiProxyClient(httpClient, cfg, NullLogger<GeminiProxyClient>.Instance);

            // Attempt 1: Loopback override without ApiKey (exfiltration attempt)
            var res1 = await client.CompleteChatAsync("sys", "user", CancellationToken.None,
                new AiCallOverrides(BaseUrl: $"http://127.0.0.1:{port}", ApiKey: null));
            Assert.False(res1.UsedLiveEndpoint);

            // Attempt 2: Loopback override even with ApiKey (SSRF attempt)
            var res2 = await client.CompleteChatAsync("sys", "user", CancellationToken.None,
                new AiCallOverrides(BaseUrl: $"https://127.0.0.1:{port}", ApiKey: "sk-caller"));
            Assert.False(res2.UsedLiveEndpoint);

            // Attempt 3: Metadata / Link-local 169.254.169.254
            var res3 = await client.CompleteChatAsync("sys", "user", CancellationToken.None,
                new AiCallOverrides(BaseUrl: "https://169.254.169.254", ApiKey: "sk-caller"));
            Assert.False(res3.UsedLiveEndpoint);

            // Attempt 4: Private network 10.0.0.1
            var res4 = await client.CompleteChatAsync("sys", "user", CancellationToken.None,
                new AiCallOverrides(BaseUrl: "https://10.0.0.1", ApiKey: "sk-caller"));
            Assert.False(res4.UsedLiveEndpoint);

            await acceptTask;
            Assert.False(receivedConnection);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task Idor_LessonsProgressPublishAndJobs_BlockedWith403ForOtherUsers()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // Register User A and User B
        var regA = await (await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"idor.a.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "User A"))).Content.ReadFromJsonAsync<JsonElement>();
        var tokenA = regA.GetProperty("accessToken").GetString()!;
        var courseAId = regA.GetProperty("personalWorkspaceCourseId").GetGuid();

        var regB = await (await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"idor.b.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "User B"))).Content.ReadFromJsonAsync<JsonElement>();
        var tokenB = regB.GetProperty("accessToken").GetString()!;

        Guid lessonAId;
        Guid jobAId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var userAId = regA.GetProperty("user").GetProperty("id").GetGuid();
            var ch = new Chapter { Id = Guid.NewGuid(), CourseId = courseAId, Title = "Private Chapter", SortOrder = 1 };
            db.Chapters.Add(ch);
            var les = new Lesson
            {
                Id = Guid.NewGuid(),
                ChapterId = ch.Id,
                Title = "Private Lesson A",
                Slug = $"priv-les-{Guid.NewGuid():N}",
                ContentAstJson = "{\"blocks\":[]}",
                PlainText = "Secret content",
                SortOrder = 100
            };
            db.Lessons.Add(les);
            lessonAId = les.Id;

            var job = new BackgroundJob
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = Guid.NewGuid(),
                UserId = userAId,
                JobType = "essay_rubric_evaluation",
                Status = "completed",
                PayloadJson = "{}"
            };
            db.BackgroundJobs.Add(job);
            jobAId = job.Id;
            await db.SaveChangesAsync();
        }

        // User B GET /lessons should NOT include User A's private lesson
        using var bLessonsReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/lessons");
        bLessonsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var bLessonsResp = await client.SendAsync(bLessonsReq);
        Assert.Equal(HttpStatusCode.OK, bLessonsResp.StatusCode);
        var bLessonsJson = await bLessonsResp.Content.ReadAsStringAsync();
        Assert.DoesNotContain(lessonAId.ToString(), bLessonsJson);

        // User B GET /lessons/{lessonAId} -> 403
        using var bGetLes = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/lessons/{lessonAId}");
        bGetLes.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(bGetLes)).StatusCode);

        // User B POST /lessons/{lessonAId}/publish-version -> 403
        using var bPubLes = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/lessons/{lessonAId}/publish-version")
        {
            Content = JsonContent.Create(new PublishLessonVersionRequest("{\"blocks\":[]}", "hacked", "hack"))
        };
        bPubLes.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(bPubLes)).StatusCode);

        // User B GET /courses/{courseAId}/progress -> 403
        using var bProgReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/courses/{courseAId}/progress");
        bProgReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(bProgReq)).StatusCode);

        // User B POST /lessons/{lessonAId}/complete -> 403
        using var bCompReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/lessons/{lessonAId}/complete");
        bCompReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(bCompReq)).StatusCode);

        // User B GET /jobs/{jobAId} -> 403
        using var bJobReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/jobs/{jobAId}");
        bJobReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(bJobReq)).StatusCode);

        // User A GET /jobs/{jobAId} -> 200
        using var aJobReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/jobs/{jobAId}");
        aJobReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(aJobReq)).StatusCode);
    }

    [Fact]
    public async Task Quiz_HidesClozeSolutionBeforeSubmit_BlocksResubmitAndInvalidMcq_EnforcesEssayQuota()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var reg = await (await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"quiz.sec.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Quiz User"))).Content.ReadFromJsonAsync<JsonElement>();
        var token = reg.GetProperty("accessToken").GetString()!;
        var userId = reg.GetProperty("user").GetProperty("id").GetGuid();

        Guid quizId, mcqItemId, essayItemId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            quizId = Guid.NewGuid();
            db.PracticeQuizzes.Add(new PracticeQuiz
            {
                Id = quizId,
                UserId = userId,
                Title = "Security Quiz",
                TotalItems = 2,
                Status = "in_progress"
            });

            mcqItemId = Guid.NewGuid();
            db.PracticeQuizItems.Add(new PracticeQuizItem
            {
                Id = mcqItemId,
                QuizId = quizId,
                ItemType = "multiple_choice",
                PromptAstJson = "{\"question\":\"1+1=?\"}",
                ChoicesJson = "[\"A. 1\",\"B. 2\",\"C. 3\"]",
                ClozeSolution = "B",
                CreatedAt = DateTimeOffset.UtcNow
            });

            essayItemId = Guid.NewGuid();
            db.PracticeQuizItems.Add(new PracticeQuizItem
            {
                Id = essayItemId,
                QuizId = quizId,
                ItemType = "essay",
                PromptAstJson = "{\"question\":\"Explain paging.\"}",
                CreatedAt = DateTimeOffset.UtcNow.AddSeconds(1)
            });

            // Pre-fill 49 AI credits used today -> 1 left, so essay (2 credits) must return 429!
            db.AiUsageLedgers.Add(new AiUsageLedger
            {
                UserId = userId,
                Feature = "explain_concept",
                CreditsCharged = 49,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }

        // 1. GET quiz before submit -> clozeSolution must be null
        using var getQuizReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/practice-quizzes/{quizId}");
        getQuizReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getQuizResp = await client.SendAsync(getQuizReq);
        Assert.Equal(HttpStatusCode.OK, getQuizResp.StatusCode);
        var quizJson = await getQuizResp.Content.ReadFromJsonAsync<JsonElement>();
        var firstItem = quizJson.GetProperty("items")[0];
        Assert.Equal(JsonValueKind.Null, firstItem.GetProperty("clozeSolution").ValueKind);

        // 2. Invalid MCQ answer ("999" or "-1") -> 400
        using var badMcqReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/practice-quizzes/{quizId}/items/{mcqItemId}/submit")
        {
            Content = JsonContent.Create(new SubmitQuizItemRequest("999", null, false))
        };
        badMcqReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(badMcqReq)).StatusCode);

        // 3. Valid MCQ answer ("B") -> 200
        using var goodMcqReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/practice-quizzes/{quizId}/items/{mcqItemId}/submit")
        {
            Content = JsonContent.Create(new SubmitQuizItemRequest("B", null, false))
        };
        goodMcqReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(goodMcqReq)).StatusCode);

        // 4. Re-submit same MCQ item -> 409 Conflict
        using var reMcqReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/practice-quizzes/{quizId}/items/{mcqItemId}/submit")
        {
            Content = JsonContent.Create(new SubmitQuizItemRequest("A", null, false))
        };
        reMcqReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.Conflict, (await client.SendAsync(reMcqReq)).StatusCode);

        // 5. Empty essay answer -> 400 BadRequest
        using var emptyEssayReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/practice-quizzes/{quizId}/items/{essayItemId}/submit")
        {
            Content = JsonContent.Create(new SubmitQuizItemRequest("   ", null, false))
        };
        emptyEssayReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(emptyEssayReq)).StatusCode);

        // 6. Valid essay answer with 49/50 credits used -> 429 TooManyRequests
        using var quotaEssayReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/practice-quizzes/{quizId}/items/{essayItemId}/submit")
        {
            Content = JsonContent.Create(new SubmitQuizItemRequest("Multi-level paging saves RAM.", null, false))
        };
        quotaEssayReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.SendAsync(quotaEssayReq)).StatusCode);
    }

    [Fact]
    public async Task Fsrs_UndoLastReview_ReplaysChronologically_AndSyncReviews_ValidatesRating400()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var reg = await (await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"fsrs.sec.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "FSRS User"))).Content.ReadFromJsonAsync<JsonElement>();
        var token = reg.GetProperty("accessToken").GetString()!;
        var userId = reg.GetProperty("user").GetProperty("id").GetGuid();

        Guid cardId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            var card = new Flashcard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CardType = "standard",
                FrontAstJson = "{\"text\":\"Q\"}",
                BackAstJson = "{\"text\":\"A\"}",
                DueAt = DateTimeOffset.UtcNow
            };
            db.Flashcards.Add(card);
            cardId = card.Id;
            await db.SaveChangesAsync();
        }

        // 1. Invalid rating 5 -> 400
        using var badSyncReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards/sync-reviews")
        {
            Content = JsonContent.Create(new SyncReviewsBatchRequest(new List<FsrsReviewEventInput>
            {
                new(Guid.NewGuid(), cardId, 5, DateTimeOffset.UtcNow, 1000)
            }))
        };
        badSyncReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(badSyncReq)).StatusCode);

        // 2. Sync 2 reviews, then undo last -> state must match replay of 1st review
        var t1 = DateTimeOffset.UtcNow.AddDays(-2);
        var t2 = DateTimeOffset.UtcNow.AddDays(-1);
        using var goodSyncReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards/sync-reviews")
        {
            Content = JsonContent.Create(new SyncReviewsBatchRequest(new List<FsrsReviewEventInput>
            {
                new(Guid.NewGuid(), cardId, 3, t1, 1200),
                new(Guid.NewGuid(), cardId, 4, t2, 900)
            }))
        };
        goodSyncReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(goodSyncReq)).StatusCode);

        using var undoReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/flashcards/{cardId}/undo-last-review");
        undoReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var undoResp = await client.SendAsync(undoReq);
        Assert.Equal(HttpStatusCode.OK, undoResp.StatusCode);
        var undoJson = await undoResp.Content.ReadFromJsonAsync<JsonElement>();

        var expectedAfter1 = FsrsV5Engine.ReplayChronologically(new[] { (3, t1) }, FsrsV5Engine.DefaultWeights);
        Assert.Equal(expectedAfter1.Stability, undoJson.GetProperty("stability").GetDouble(), 4);
        Assert.Equal(expectedAfter1.Reps, undoJson.GetProperty("reps").GetInt32());
        Assert.Equal(expectedAfter1.State, undoJson.GetProperty("state").GetInt32());
    }

    [Fact]
    public async Task WizardCommit_EmptyChapters400_AndDoubleSubmitIdempotent_AndCorruptZip400()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var reg = await (await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"wiz.sec.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Wiz User"))).Content.ReadFromJsonAsync<JsonElement>();
        var token = reg.GetProperty("accessToken").GetString()!;

        // 1. Empty chapters -> 400
        using var emptyWizReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/courses/generate-wizard/commit")
        {
            Content = JsonContent.Create(new { topic = "OS", title = "T", description = "D", chapters = Array.Empty<object>() })
        };
        emptyWizReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(emptyWizReq)).StatusCode);

        // 2. Concurrent double-submit with same IdempotencyKey -> creates only 1 course
        var idemp = Guid.NewGuid();
        var commitBody = new
        {
            topic = "Distributed Systems",
            title = "DS 101",
            description = "Desc",
            chapters = new[]
            {
                new { title = "Ch1", sortOrder = 1, lessons = new[] { new { title = "L1", summary = "Sum", estimatedMinutes = 10 } } }
            },
            idempotencyKey = idemp
        };

        async Task<Guid> SendCommitAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/courses/generate-wizard/commit")
            {
                Content = JsonContent.Create(commitBody)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("id").GetGuid();
        }

        var ids = await Task.WhenAll(SendCommitAsync(), SendCommitAsync());
        Assert.Equal(ids[0], ids[1]);

        // 3. Corrupt zip -> 400 BadRequest without stack trace
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("not-a-valid-zip-file")), "file", "corrupt.zip");
        using var zipReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/courses/import-zip") { Content = form };
        zipReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var zipResp = await client.SendAsync(zipReq);
        Assert.Equal(HttpStatusCode.BadRequest, zipResp.StatusCode);
        var zipErr = await zipResp.Content.ReadAsStringAsync();
        Assert.DoesNotContain("System.IO.InvalidDataException", zipErr);
    }

    [Fact]
    public async Task BoundaryValidation_JsonUpload_Annotations_Notebooks_Enforced()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var reg = await (await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"bnd.sec.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Boundary User"))).Content.ReadFromJsonAsync<JsonElement>();
        var token = reg.GetProperty("accessToken").GetString()!;

        // 1. JSON /documents/upload with FileSizeBytes <= 0 -> 400
        using var badUploadReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/documents/upload")
        {
            Content = JsonContent.Create(new { title = "Bad", fileSizeBytes = 0, totalPages = 1, extractedText = "hello" })
        };
        badUploadReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(badUploadReq)).StatusCode);

        // 2. POST /annotations with StartOffset < 0 -> 400
        using var badAnnReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/annotations")
        {
            Content = JsonContent.Create(new CreateAnnotationRequest(
                Guid.NewGuid(), null, null, null, null, "Quote", null, null, -5, 5, "yellow", null, "Note"))
        };
        badAnnReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(badAnnReq)).StatusCode);

        // 3. PUT /notebooks/{id} with ContentAstJson > 2_000_000 -> 413 or 400
        using var createNbReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notebooks")
        {
            Content = JsonContent.Create(new CreateNotebookRequest("Test NB"))
        };
        createNbReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var nbJson = await (await client.SendAsync(createNbReq)).Content.ReadFromJsonAsync<JsonElement>();
        var nbId = nbJson.GetProperty("id").GetGuid();

        var hugeContent = new string('x', 2_000_005);
        using var hugeNbReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/notebooks/{nbId}")
        {
            Content = JsonContent.Create(new SaveNotebookRequest(null, 1, "web", "Test NB", hugeContent))
        };
        hugeNbReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var hugeResp = await client.SendAsync(hugeNbReq);
        Assert.True(hugeResp.StatusCode == HttpStatusCode.RequestEntityTooLarge || hugeResp.StatusCode == HttpStatusCode.BadRequest);
    }

    private sealed class StubJwksHttpMessageHandler(string jwksJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jwksJson, Encoding.UTF8, "application/json")
            };
            resp.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromMinutes(10) };
            return Task.FromResult(resp);
        }
    }

    private sealed class StubGoogleHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private static string SignGoogleTestJwt(
        RSA rsa,
        string keyId,
        string clientId,
        string subject,
        string email,
        bool emailVerified,
        string fullName = "Google Learner",
        string issuer = "https://accounts.google.com",
        DateTimeOffset? expiresAt = null)
    {
        var securityKey = new RsaSecurityKey(rsa) { KeyId = keyId };
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        var claims = new[]
        {
            new Claim("sub", subject),
            new Claim("email", email),
            new Claim("email_verified", emailVerified ? "true" : "false"),
            new Claim("name", fullName),
            new Claim("picture", "https://lh3.googleusercontent.com/a/test-avatar")
        };

        var now = DateTime.UtcNow;
        var exp = (expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(10)).UtcDateTime;
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: clientId,
            claims: claims,
            notBefore: exp.AddMinutes(-15) < now ? exp.AddMinutes(-15) : now.AddMinutes(-1),
            expires: exp,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task GoogleAuth_UnconfiguredReturns503_AndRs256JwksValidation_Linking_ChangePassword_Enforced()
    {
        // 1. Unconfigured Google Client ID -> googleEnabled = false, POST /auth/google -> 503
        await using (var unconfiguredFactory = CreateIsolatedFactory(enforceStrictBearer: true))
        {
            using var unconfiguredClient = unconfiguredFactory.CreateClient();
            var provResp = await unconfiguredClient.GetAsync("/api/v1/auth/providers");
            Assert.Equal(HttpStatusCode.OK, provResp.StatusCode);
            var provJson = await provResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(provJson.GetProperty("googleEnabled").GetBoolean());

            var disabledGoogleResp = await unconfiguredClient.PostAsJsonAsync("/api/v1/auth/google",
                new GoogleAuthRequest("fake.jwt.token"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, disabledGoogleResp.StatusCode);
        }

        // 2. Configured Google Client ID with real RSA-2048 JWKS verification
        using var rsa = RSA.Create(2048);
        const string keyId = "google-test-kid-1";
        const string clientId = "1234567890-studxytest.apps.googleusercontent.com";
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(rsa) { KeyId = keyId });
        var jwksJson = JsonSerializer.Serialize(new
        {
            keys = new[]
            {
                new { kty = jwk.Kty, use = "sig", kid = keyId, alg = "RS256", n = jwk.N, e = jwk.E }
            }
        });

        using var mockHttpClient = new HttpClient(new StubJwksHttpMessageHandler(jwksJson));
        var dbName = $"SwmsGoogleAuthDb_{Guid.NewGuid():N}";

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
                builder.UseSetting("Auth:EnforceStrictBearer", "true");
                builder.UseSetting("Authentication:Google:ClientId", clientId);
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IGoogleIdTokenValidator>(sp =>
                        new GoogleJwksIdTokenValidator(
                            sp.GetRequiredService<IConfiguration>(),
                            new StubGoogleHttpClientFactory(mockHttpClient),
                            NullLogger<GoogleJwksIdTokenValidator>.Instance));
                });
            });

        using var client = factory.CreateClient();

        // Check /auth/providers returns googleEnabled=true and primary clientId
        var activeProv = await (await client.GetAsync("/api/v1/auth/providers")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(activeProv.GetProperty("googleEnabled").GetBoolean());
        Assert.Equal(clientId, activeProv.GetProperty("googleClientId").GetString());

        // Reject unverified email -> 401
        var unverifiedJwt = SignGoogleTestJwt(rsa, keyId, clientId, "sub-unverified", "unverified@studxy.vn", emailVerified: false);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleAuthRequest(unverifiedJwt))).StatusCode);

        // Reject wrong audience -> 401
        var wrongAudJwt = SignGoogleTestJwt(rsa, keyId, "other-app.apps.googleusercontent.com", "sub-wrong-aud", "aud@studxy.vn", emailVerified: true);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleAuthRequest(wrongAudJwt))).StatusCode);

        // Reject expired token -> 401
        var expiredJwt = SignGoogleTestJwt(rsa, keyId, clientId, "sub-expired", "exp@studxy.vn", emailVerified: true,
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(-10));
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleAuthRequest(expiredJwt))).StatusCode);

        // Reject forged signature -> 401
        using var rogueRsa = RSA.Create(2048);
        var forgedJwt = SignGoogleTestJwt(rogueRsa, keyId, clientId, "sub-forged", "forged@studxy.vn", emailVerified: true);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleAuthRequest(forgedJwt))).StatusCode);

        // Valid Google ID token creates new Google-only user + personal workspace
        var googleEmail = $"google.user.{Guid.NewGuid():N}@studxy.vn";
        var googleSub = $"google-sub-{Guid.NewGuid():N}";
        var validJwt = SignGoogleTestJwt(rsa, keyId, clientId, googleSub, googleEmail, emailVerified: true, fullName: "Nguyễn Google");
        var googleResp = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleAuthRequest(validJwt));
        Assert.Equal(HttpStatusCode.OK, googleResp.StatusCode);
        var googleJson = await googleResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(googleJson.GetProperty("isNewUser").GetBoolean());
        var googleAccessToken = googleJson.GetProperty("accessToken").GetString()!;
        var gUser = googleJson.GetProperty("user");
        Assert.Equal("google", gUser.GetProperty("authProvider").GetString());
        Assert.False(gUser.GetProperty("hasLocalPassword").GetBoolean());

        // Password login with sentinel or empty password must fail 401
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(googleEmail, PasswordHashUtility.ExternalOnlySentinel))).StatusCode);

        // Google-only user can set an initial password -> upgrades to local+google & rotates TokenVersion
        using var setPwdReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/change-password")
        {
            Content = JsonContent.Create(new ChangePasswordRequest(null, "GoogleUserPass123!"))
        };
        setPwdReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", googleAccessToken);
        var setPwdResp = await client.SendAsync(setPwdReq);
        Assert.Equal(HttpStatusCode.OK, setPwdResp.StatusCode);
        var setPwdJson = await setPwdResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("local+google", setPwdJson.GetProperty("user").GetProperty("authProvider").GetString());
        Assert.True(setPwdJson.GetProperty("user").GetProperty("hasLocalPassword").GetBoolean());

        // Old token revoked after TokenVersion increment -> 401
        using var staleMeReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        staleMeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", googleAccessToken);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(staleMeReq)).StatusCode);

        // Local email+password login now works!
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(googleEmail, "GoogleUserPass123!"))).StatusCode);

        // Pre-existing local user signing in with Google (verified email) links cleanly -> local+google
        var localEmail = $"local.link.{Guid.NewGuid():N}@studxy.vn";
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/register",
                new RegisterRequest(localEmail, "LocalPass123!", "Local First"))).StatusCode);

        var linkJwt = SignGoogleTestJwt(rsa, keyId, clientId, $"sub-link-{Guid.NewGuid():N}", localEmail, emailVerified: true);
        var linkResp = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleAuthRequest(linkJwt));
        Assert.Equal(HttpStatusCode.OK, linkResp.StatusCode);
        var linkJson = await linkResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(linkJson.GetProperty("isNewUser").GetBoolean());
        Assert.Equal("local+google", linkJson.GetProperty("user").GetProperty("authProvider").GetString());
    }

    [Fact]
    public async Task SqliteSchemaMigration_UpgradesLegacyUsersTable_WithoutError()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE users (
                    id TEXT NOT NULL PRIMARY KEY,
                    email TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    full_name TEXT NOT NULL,
                    role TEXT NOT NULL,
                    token_version INTEGER NOT NULL DEFAULT 1,
                    streak_days INTEGER NOT NULL DEFAULT 1,
                    onboarding_step INTEGER NOT NULL DEFAULT 1,
                    is_activated INTEGER NOT NULL DEFAULT 0,
                    activated_at TEXT,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );
                CREATE TABLE lessons (
                    id TEXT NOT NULL PRIMARY KEY,
                    chapter_id TEXT NOT NULL,
                    title TEXT NOT NULL,
                    slug TEXT NOT NULL,
                    sort_order INTEGER NOT NULL DEFAULT 0,
                    content_ast TEXT NOT NULL,
                    plain_text TEXT NOT NULL,
                    current_version INTEGER NOT NULL DEFAULT 1,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );";
            await cmd.ExecuteNonQueryAsync();
        }

        var opts = new DbContextOptionsBuilder<SwmsDbContext>().UseSqlite(conn).Options;
        await using var db = new SwmsDbContext(opts);
        await SwmsEndpoints.EnsureSqliteSchemaUpdatedAsync(db);

        db.Users.Add(new User
        {
            Email = "legacy.upgrade@studxy.vn",
            PasswordHash = PasswordHashUtility.HashPassword("Studxy@2026"),
            FullName = "Legacy Upgraded",
            AuthProvider = "google",
            GoogleSub = "legacy-sub-123"
        });
        await db.SaveChangesAsync();

        var loaded = await db.Users.FirstOrDefaultAsync(u => u.GoogleSub == "legacy-sub-123");
        Assert.NotNull(loaded);
        Assert.Equal("google", loaded!.AuthProvider);
    }
}
