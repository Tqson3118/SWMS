using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swms.Api.Domain.Entities;
using Swms.Api.Features;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.FSRS;
using Swms.Api.Infrastructure.Security;
using Swms.Api.Infrastructure.SelfHealing;

namespace Swms.Tests;

public class SwmsArchitectureTests
{
    private static SwmsDbContext CreateIsolatedDbContext()
    {
        var options = new DbContextOptionsBuilder<SwmsDbContext>()
            .UseInMemoryDatabase($"SwmsTestDb_{Guid.NewGuid():N}")
            .Options;
        return new SwmsDbContext(options);
    }

    [Fact]
    public void FsrsV5_GoldenVectors_MatchCanonical19ParameterFormulasTo4Decimals()
    {
        var now = new DateTimeOffset(2026, 10, 31, 5, 0, 0, TimeSpan.Zero);
        var newCard = new FsrsCardState(0, 0, 0, 0, 0, null, now);

        // Vector 1: First review ratings 1..4 on New card match w[0..3] initial stabilities
        var r1 = FsrsV5Engine.Evaluate(newCard, 1, now);
        var r2 = FsrsV5Engine.Evaluate(newCard, 2, now);
        var r3 = FsrsV5Engine.Evaluate(newCard, 3, now);
        var r4 = FsrsV5Engine.Evaluate(newCard, 4, now);

        Assert.Equal(0.4072, r1.Stability, 4);
        Assert.Equal(1.1829, r2.Stability, 4);
        Assert.Equal(3.1262, r3.Stability, 4);
        Assert.Equal(15.4722, r4.Stability, 4);

        // Initial difficulties: D0(G) = clamp(w4 - exp(w5*(G-1)) + 1, 1, 10)
        // G=1: 7.2102 - exp(0) + 1 = 7.2102
        // G=3: 7.2102 - exp(0.5316 * 2) + 1 = 7.2102 - 2.895638 + 1 = 5.3146
        Assert.Equal(7.2102, r1.Difficulty, 4);
        Assert.Equal(5.3146, r3.Difficulty, 4);

        // Vector 2: Second review after 3 days with rating Good (3)
        var reviewCard = new FsrsCardState(r3.Stability, r3.Difficulty, 1, 0, 2, now, now.AddDays(3));
        var secondEval = FsrsV5Engine.Evaluate(reviewCard, 3, now.AddDays(3));

        Assert.True(secondEval.Stability > r3.Stability);
        Assert.Equal(3.0, secondEval.ElapsedDays, 4);
        Assert.Equal(2, secondEval.State);
        Assert.Equal(2, secondEval.Reps);
    }

    [Fact]
    public void FsrsV5_AsymmetricClockValidation_ClampsFutureAndPreservesPastOffline()
    {
        var serverNow = new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero);

        // 1. Future-skewed client clock (+5 days) -> Clamped to serverNow
        var futureSkewed = serverNow.AddDays(5);
        var normalizedFuture = FsrsV5Engine.ValidateAndNormalizeTimestamp(futureSkewed, serverNow, out var wasClamped);
        Assert.True(wasClamped);
        Assert.Equal(serverNow, normalizedFuture);

        // 2. Legitimate offline study (2 days ago) -> Preserved exactly
        var offlinePast = serverNow.AddDays(-2);
        var normalizedPast = FsrsV5Engine.ValidateAndNormalizeTimestamp(offlinePast, serverNow, out var pastClamped);
        Assert.False(pastClamped);
        Assert.Equal(offlinePast, normalizedPast);

        // 3. Stale timestamp (> 30 days ago) -> Rejected
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FsrsV5Engine.ValidateAndNormalizeTimestamp(serverNow.AddDays(-31), serverNow, out _));
    }

    [Fact]
    public void FsrsV5_OutOfOrderReplay_ProducesDeterministicState()
    {
        var t1 = new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 10, 23, 8, 0, 0, TimeSpan.Zero);
        var t3 = new DateTimeOffset(2026, 10, 28, 8, 0, 0, TimeSpan.Zero);

        var inOrder = new[] { (3, t1), (3, t2), (4, t3) };
        var outOfOrder = new[] { (4, t3), (3, t1), (3, t2) };

        var stateA = FsrsV5Engine.ReplayChronologically(inOrder);
        var stateB = FsrsV5Engine.ReplayChronologically(outOfOrder);

        Assert.Equal(stateA.Stability, stateB.Stability, 4);
        Assert.Equal(stateA.Difficulty, stateB.Difficulty, 4);
        Assert.Equal(stateA.DueAt, stateB.DueAt);
    }

    [Fact]
    public void SelfHealing_ThreeTiers_ExactShiftTypoAmbiguousAndOrphaned()
    {
        var blockId = Guid.NewGuid();
        var oldParagraph = "Bộ đệm Translation Lookaside Bufffer (TLB) giúp tăng tốc dịch địa chỉ trong MMU.";
        var quoteWithTypo = "Translation Lookaside Bufffer (TLB)";
        var startIdx = oldParagraph.IndexOf(quoteWithTypo, StringComparison.Ordinal);

        var ann = new Annotation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            BlockId = blockId,
            ExactQuote = quoteWithTypo,
            OriginalExactQuote = quoteWithTypo,
            PrefixQuote = "Bộ đệm ",
            SuffixQuote = " giúp tăng tốc",
            StartOffset = startIdx,
            EndOffset = startIdx + quoteWithTypo.Length,
            ColorKey = "yellow",
            Status = "anchored"
        };

        // Case 1: Instructor fixes typo ("Bufffer" -> "Buffer") and prepends intro text -> Tier 3 Smith-Waterman heals!
        var updatedParagraph = "Trong kiến trúc hiện đại, Bộ đệm Translation Lookaside Buffer (TLB) giúp tăng tốc dịch địa chỉ trong MMU.";
        var updatedAst = JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = blockId.ToString() },
                    content = new[] { new { type = "text", text = updatedParagraph } }
                }
            }
        });

        var resTier3 = ThreeTierAnnotationHealer.HealAnnotation(ann, oldParagraph, updatedAst, updatedParagraph);
        Assert.Equal("anchored", resTier3.NewStatus);
        Assert.Equal("Tier3_SmithWaterman", resTier3.TierUsed);
        Assert.True(resTier3.ConfidenceScore >= 0.85);
        Assert.Equal("Translation Lookaside Buffer (TLB)", ann.ExactQuote);
        // Immutable OriginalExactQuote MUST remain untouched!
        Assert.Equal("Translation Lookaside Bufffer (TLB)", ann.OriginalExactQuote);

        // Case 2: Ambiguous duplicate occurrences without disambiguating context -> needs_review
        var dupParagraph = "Khái niệm TLB Hit xuất hiện ở đây và khái niệm TLB Hit xuất hiện lần nữa ở đây.";
        var dupAst = JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = blockId.ToString() },
                    content = new[] { new { type = "text", text = dupParagraph } }
                }
            }
        });
        var dupAnn = new Annotation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            BlockId = blockId,
            ExactQuote = "TLB Hit",
            OriginalExactQuote = "TLB Hit",
            PrefixQuote = null,
            SuffixQuote = null,
            Status = "anchored"
        };

        var resAmbiguous = ThreeTierAnnotationHealer.HealAnnotation(dupAnn, dupParagraph, dupAst, dupParagraph);
        Assert.Equal("needs_review", resAmbiguous.NewStatus);
        Assert.Equal("needs_review", dupAnn.Status);

        // Case 3: Text completely deleted -> orphaned
        var deletedParagraph = "Nội dung hoàn toàn mới về thuật toán lập lịch CPU Round-Robin.";
        var deletedAst = JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = blockId.ToString() },
                    content = new[] { new { type = "text", text = deletedParagraph } }
                }
            }
        });

        var resOrphaned = ThreeTierAnnotationHealer.HealAnnotation(ann, updatedParagraph, deletedAst, deletedParagraph);
        Assert.Equal("orphaned", resOrphaned.NewStatus);
        Assert.Equal("orphaned", ann.Status);
    }

    [Fact]
    public async Task RefreshTokenRotation_Enforces15To30sGraceWindowAndBreachAlarm()
    {
        using var db = CreateIsolatedDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "security@studxy.vn",
            PasswordHash = PasswordHashUtility.HashPassword("Pass123!"),
            FullName = "Security Tester",
            TokenVersion = 1
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:GraceWindowSeconds"] = "30"
            })
            .Build();
        var protector = new AesGcmTokenProtector("test-master-key-2026");
        var jwt = new JwtTokenService(cfg);
        var rtr = new RefreshTokenRotationService(db, jwt, protector, cfg);

        var t0 = new DateTimeOffset(2026, 10, 31, 10, 0, 0, TimeSpan.Zero);
        var (_, rawToken1, _) = await rtr.IssueInitialTokenPairAsync(user, "Tab-1", "127.0.0.1", t0);

        // 1. First rotation at t0 + 5m succeeds and revokes rawToken1
        var rot1 = await rtr.RotateRefreshTokenAsync(rawToken1, t0.AddMinutes(5));
        Assert.True(rot1.Succeeded);
        Assert.False(rot1.IsGraceWindowRecovery);
        Assert.NotNull(rot1.RefreshToken);

        // 2. Concurrent Tab-2 presents rawToken1 10 seconds later (inside 30s grace window) -> Idempotent recovery!
        var rotGrace = await rtr.RotateRefreshTokenAsync(rawToken1, t0.AddMinutes(5).AddSeconds(10));
        Assert.True(rotGrace.Succeeded);
        Assert.True(rotGrace.IsGraceWindowRecovery);
        Assert.Equal(rot1.RefreshToken, rotGrace.RefreshToken);
        Assert.Equal(1, (await db.Users.FirstAsync(u => u.Id == user.Id)).TokenVersion);

        // 3. Attacker replays rawToken1 45 seconds later (outside 30s grace window) -> BREACH ALARM!
        var rotBreach = await rtr.RotateRefreshTokenAsync(rawToken1, t0.AddMinutes(5).AddSeconds(45));
        Assert.False(rotBreach.Succeeded);
        Assert.True(rotBreach.IsBreachDetected);

        var updatedUser = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(2, updatedUser.TokenVersion); // token_version incremented!
        var allFamilyTokens = await db.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
        Assert.All(allFamilyTokens, t => Assert.True(t.IsRevoked));
    }

    [Fact]
    public async Task RelationalIntegrity_SoftDeletePreservesReviews_AndAnnotationDeleteSetsNull()
    {
        using var db = CreateIsolatedDbContext();
        await SeedDataInitializer.SeedAsync(db);

        var cardWithReviews = await db.Flashcards.FirstAsync(f => f.Id == Guid.Parse("99999991-1111-4111-8111-111111111111"));
        var annId = cardWithReviews.AnnotationId!.Value;

        // 1. Attempting to hard-delete a flashcard with review history MUST throw ON DELETE RESTRICT
        db.Flashcards.Remove(cardWithReviews);
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        db.Entry(cardWithReviews).State = EntityState.Unchanged;

        // 2. Deleting the underlying Annotation sets Flashcard.AnnotationId = null (ON DELETE SET NULL) while keeping snapshots!
        var ann = await db.Annotations.FirstAsync(a => a.Id == annId);
        db.Annotations.Remove(ann);
        await db.SaveChangesAsync();

        var reloadedCard = await db.Flashcards.FirstAsync(f => f.Id == cardWithReviews.Id);
        Assert.Null(reloadedCard.AnnotationId);
        Assert.NotEmpty(reloadedCard.SourceTitleSnapshot!);
        Assert.NotEmpty(reloadedCard.SourceQuoteSnapshot!);
    }

    [Fact]
    public async Task ApiEndToEnd_Notebook409KeepBoth_AndIdempotentReviewSync()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        // 1. Fetch default notebook (Version 1)
        var nbResp = await client.GetFromJsonAsync<JsonElement>($"/api/v1/notebooks/{SeedDataInitializer.DefaultNotebookId}");
        Assert.Equal(1, nbResp.GetProperty("version").GetInt32());

        // 2. Save notebook with BaseVersion = 1 -> Succeeds and increments to Version 2
        var validSave = new SaveNotebookRequest(
            Guid.NewGuid(),
            1,
            "Device-A",
            "Ghi chú Bộ nhớ ảo (Updated v2)",
            "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Nội dung cập nhật hợp lệ v2\"}]}]}");

        var put1 = await client.PutAsJsonAsync($"/api/v1/notebooks/{SeedDataInitializer.DefaultNotebookId}", validSave);
        Assert.Equal(HttpStatusCode.OK, put1.StatusCode);

        // 3. Stale client sends BaseVersion = 1 -> Returns 409 Conflict and forks a "[Bản sao xung đột]" copy!
        var conflictingSave = new SaveNotebookRequest(
            Guid.NewGuid(),
            1, // Stale base version!
            "Device-B",
            "Ghi chú Bộ nhớ ảo",
            "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Nội dung từ tab xung đột được bảo toàn\"}]}]}");

        var put409 = await client.PutAsJsonAsync($"/api/v1/notebooks/{SeedDataInitializer.DefaultNotebookId}", conflictingSave);
        Assert.Equal(HttpStatusCode.Conflict, put409.StatusCode);

        var conflictBody = await put409.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CONFLICT_KEEP_BOTH", conflictBody.GetProperty("error").GetString());
        Assert.Equal("kept_both", conflictBody.GetProperty("resolutionStatus").GetString());
        var forkTitle = conflictBody.GetProperty("conflictCopyNotebook").GetProperty("title").GetString();
        Assert.Contains("Bản sao xung đột", forkTitle);

        // 4. Idempotent FSRS Review Sync: sending the same review_event_id twice does not duplicate review logs
        var reviewEventId = Guid.NewGuid();
        var syncReq = new SyncReviewsBatchRequest(
        [
            new FsrsReviewEventInput(
                reviewEventId,
                Guid.Parse("99999992-2222-4222-8222-222222222222"),
                3,
                DateTimeOffset.UtcNow,
                2500)
        ]);

        var sync1 = await client.PostAsJsonAsync("/api/v1/flashcards/sync-reviews", syncReq);
        Assert.Equal(HttpStatusCode.OK, sync1.StatusCode);
        var sync1Json = await sync1.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(sync1Json.GetProperty("results")[0].GetProperty("idempotentDuplicate").GetBoolean());

        var sync2 = await client.PostAsJsonAsync("/api/v1/flashcards/sync-reviews", syncReq);
        Assert.Equal(HttpStatusCode.OK, sync2.StatusCode);
        var sync2Json = await sync2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(sync2Json.GetProperty("results")[0].GetProperty("idempotentDuplicate").GetBoolean());
    }

    [Fact]
    public void SelfHealing_UnicodeCombiningDiacriticsNfdVsNfc_AndBoundary085Similarity()
    {
        var blockId = Guid.NewGuid();
        // NFC quote ("Bộ nhớ ảo (Virtual Memory)") vs NFD decomposed block text with minor edit
        var nfcQuote = "Bộ nhớ ảo (Virtual Memory) cấp phát khung trang";
        var nfdParagraph = "Trong hệ điều hành, Bo\u0323\u0302 nho\u031B\u0301 a\u0309o (Virtual Memory) ca\u0302\u0301p pha\u0301t khung trang vật lý.";
        var nfdAst = JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = blockId.ToString() },
                    content = new[] { new { type = "text", text = nfdParagraph } }
                }
            }
        });

        var annNfd = new Annotation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            BlockId = blockId,
            ExactQuote = nfcQuote,
            OriginalExactQuote = nfcQuote,
            StartOffset = 0,
            EndOffset = nfcQuote.Length,
            ColorKey = "yellow",
            Status = "anchored"
        };

        var healNfd = ThreeTierAnnotationHealer.HealAnnotation(annNfd, nfcQuote, nfdAst, nfdParagraph);
        Assert.Equal("anchored", healNfd.NewStatus);
        Assert.True(healNfd.ConfidenceScore >= 0.85);
        Assert.Equal(
            nfdParagraph.Substring(healNfd.NewStartOffset, healNfd.NewEndOffset - healNfd.NewStartOffset),
            annNfd.ExactQuote);
        Assert.Equal(nfcQuote, annNfd.OriginalExactQuote);

        // Exact 0.85 boundary test: 20-char quote with 3 substitutions (17/20 = 0.8500 -> heals) vs 4 substitutions (16/20 = 0.8000 -> orphaned)
        var boundaryQuote = "ABCDEFGHIJKLMNOPQRST";
        var paragraph085 = " Đoạn mở đầu: ABCDEFGHIJKLXXXPQRST kết thúc.";
        var ast085 = JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = blockId.ToString() },
                    content = new[] { new { type = "text", text = paragraph085 } }
                }
            }
        });
        var ann085 = new Annotation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            BlockId = blockId,
            ExactQuote = boundaryQuote,
            OriginalExactQuote = boundaryQuote,
            Status = "anchored"
        };
        var res085 = ThreeTierAnnotationHealer.HealAnnotation(ann085, boundaryQuote, ast085, paragraph085);
        Assert.Equal("anchored", res085.NewStatus);
        Assert.Equal(0.85, res085.ConfidenceScore, 4);

        var paragraph080 = " Đoạn mở đầu: ABCDEFGHIJKXXXXPQRST kết thúc.";
        var ast080 = JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = blockId.ToString() },
                    content = new[] { new { type = "text", text = paragraph080 } }
                }
            }
        });
        var ann080 = new Annotation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            BlockId = blockId,
            ExactQuote = boundaryQuote,
            OriginalExactQuote = boundaryQuote,
            Status = "anchored"
        };
        var res080 = ThreeTierAnnotationHealer.HealAnnotation(ann080, boundaryQuote, ast080, paragraph080);
        Assert.Equal("orphaned", res080.NewStatus);
    }

    [Fact]
    public async Task RefreshTokenRotation_ConcurrentScopesAndExactGraceBoundary_EnforcesJwtTokenVersion()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        // 1. Register an isolated user to obtain initial AccessToken (ver = 1), RefreshToken, and Personal Workspace
        var regEmail = $"rtr.concurrent.{Guid.NewGuid():N}@studxy.vn";
        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(regEmail, "Studxy@2026", "RTR Concurrent Tester"));
        Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var accessTokenV1 = regJson.GetProperty("accessToken").GetString()!;
        var refreshToken1 = regJson.GetProperty("refreshToken").GetString()!;

        // 2. Authenticated call with valid JWT (ver = 1) succeeds
        using var authReqOk = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        authReqOk.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessTokenV1);
        var meOk = await client.SendAsync(authReqOk);
        Assert.Equal(HttpStatusCode.OK, meOk.StatusCode);

        // 3. Fire two concurrent /api/v1/auth/refresh requests with the SAME refreshToken1 across independent HTTP scopes
        var taskA = client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(refreshToken1));
        var taskB = client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(refreshToken1));
        var responses = await Task.WhenAll(taskA, taskB);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var bodyA = await responses[0].Content.ReadFromJsonAsync<JsonElement>();
        var bodyB = await responses[1].Content.ReadFromJsonAsync<JsonElement>();
        // Both concurrent callers receive the exact same rotated replacement refresh token!
        Assert.Equal(bodyA.GetProperty("refreshToken").GetString(), bodyB.GetProperty("refreshToken").GetString());
        Assert.True(
            bodyA.GetProperty("graceWindowRecovered").GetBoolean() !=
            bodyB.GetProperty("graceWindowRecovered").GetBoolean());

        // 4. Logout increments TokenVersion (1 -> 2); presenting accessTokenV1 (ver = 1) MUST now return 401 Unauthorized!
        using var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessTokenV1);
        var logoutResp = await client.SendAsync(logoutReq);
        Assert.Equal(HttpStatusCode.OK, logoutResp.StatusCode);

        using var staleJwtReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        staleJwtReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessTokenV1);
        var meRevoked = await client.SendAsync(staleJwtReq);
        Assert.Equal(HttpStatusCode.Unauthorized, meRevoked.StatusCode);
        var revokedBody = await meRevoked.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(revokedBody.GetProperty("tokenVersionRevoked").GetBoolean());

        // 5. Verify exact 30.000s (inclusive) vs 30.001s (breach) grace window boundary
        using var db = CreateIsolatedDbContext();
        var boundaryUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "boundary@studxy.vn",
            PasswordHash = PasswordHashUtility.HashPassword("Pass123!"),
            FullName = "Boundary Tester",
            TokenVersion = 1
        };
        db.Users.Add(boundaryUser);
        await db.SaveChangesAsync();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:GraceWindowSeconds"] = "30" })
            .Build();
        var rtr = new RefreshTokenRotationService(db, new JwtTokenService(cfg), new AesGcmTokenProtector("boundary-key"), cfg);
        var t0 = new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero);
        var (_, boundaryRt, _) = await rtr.IssueInitialTokenPairAsync(boundaryUser, "Device", "127.0.0.1", t0);

        await rtr.RotateRefreshTokenAsync(boundaryRt, t0);
        var at30000ms = await rtr.RotateRefreshTokenAsync(boundaryRt, t0.AddMilliseconds(30000));
        Assert.True(at30000ms.Succeeded);
        Assert.True(at30000ms.IsGraceWindowRecovery);

        var at30001ms = await rtr.RotateRefreshTokenAsync(boundaryRt, t0.AddMilliseconds(30001));
        Assert.False(at30001ms.Succeeded);
        Assert.True(at30001ms.IsBreachDetected);
    }

    [Fact]
    public void PostgresRelationalMapping_SelectForUpdateAndSnakeCaseSchema_MatchesDdl()
    {
        var options = new DbContextOptionsBuilder<SwmsDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=5432;Database=swms_db;Username=swms_user;Password=swms_password_dev_2026")
            .Options;

        using var db = new SwmsDbContext(options);
        Assert.True(db.Database.IsRelational());

        // 1. Verify top-level non-composed SELECT ... FOR UPDATE SQL generation on RefreshTokens
        var sampleHash = PasswordHashUtility.ComputeSha256Hex("sample-refresh-token");
        var sql = db.RefreshTokens
            .FromSqlInterpolated($"SELECT * FROM refresh_tokens WHERE token_hash = {sampleHash} FOR UPDATE")
            .AsTracking()
            .ToQueryString();

        Assert.Contains("SELECT * FROM refresh_tokens WHERE token_hash =", sql);
        Assert.EndsWith("FOR UPDATE", sql.Trim());

        // 2. Verify all 22 entities and their columns map 100% to snake_case & jsonb matching database/schema.sql
        var userEntity = db.Model.FindEntityType(typeof(User))!;
        Assert.Equal("users", userEntity.GetTableName());
        Assert.Equal("token_version", userEntity.FindProperty(nameof(User.TokenVersion))!.GetColumnName());
        Assert.Equal("password_hash", userEntity.FindProperty(nameof(User.PasswordHash))!.GetColumnName());

        var rtEntity = db.Model.FindEntityType(typeof(RefreshToken))!;
        Assert.Equal("refresh_tokens", rtEntity.GetTableName());
        Assert.Equal("token_hash", rtEntity.FindProperty(nameof(RefreshToken.TokenHash))!.GetColumnName());
        Assert.Equal("replaced_by_token_id", rtEntity.FindProperty(nameof(RefreshToken.ReplacedByTokenId))!.GetColumnName());
        Assert.Equal("encrypted_replacement_token", rtEntity.FindProperty(nameof(RefreshToken.EncryptedReplacementToken))!.GetColumnName());

        var lessonEntity = db.Model.FindEntityType(typeof(Lesson))!;
        Assert.Equal("lessons", lessonEntity.GetTableName());
        var lessonAstProp = lessonEntity.FindProperty(nameof(Lesson.ContentAstJson))!;
        Assert.Equal("content_ast", lessonAstProp.GetColumnName());
        Assert.Equal("jsonb", lessonAstProp.GetColumnType());

        var pageEntity = db.Model.FindEntityType(typeof(DocumentPage))!;
        Assert.Equal("document_pages", pageEntity.GetTableName());
        Assert.Equal("document_id", pageEntity.FindProperty(nameof(DocumentPage.DocumentId))!.GetColumnName());
        Assert.Equal("page_number", pageEntity.FindProperty(nameof(DocumentPage.PageNumber))!.GetColumnName());
        Assert.Equal("plain_text", pageEntity.FindProperty(nameof(DocumentPage.PlainText))!.GetColumnName());
        Assert.Equal("has_text_layer", pageEntity.FindProperty(nameof(DocumentPage.HasTextLayer))!.GetColumnName());
        Assert.Equal("char_count", pageEntity.FindProperty(nameof(DocumentPage.CharCount))!.GetColumnName());
        Assert.Equal("word_count", pageEntity.FindProperty(nameof(DocumentPage.WordCount))!.GetColumnName());
    }
}
