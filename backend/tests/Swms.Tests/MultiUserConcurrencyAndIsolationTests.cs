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
using Swms.Api.Infrastructure.FSRS;
using Xunit;

namespace Swms.Tests;

public class MultiUserConcurrencyAndIsolationTests
{
    private static WebApplicationFactory<Program> CreateIsolatedFactory()
    {
        var dbName = $"SwmsMultiUserTestDb_{Guid.NewGuid():N}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
            });
    }

    private static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, "test-fixtures")) && Directory.Exists(Path.Combine(dir, "test")))
            {
                return dir;
            }
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        throw new DirectoryNotFoundException("Could not find repository root containing 'test-fixtures' and 'test'.");
    }

    [Fact]
    public async Task Test1_MultiTenantIsolation_UserBCannotAccessUserAPrivateResources_WhileDefaultSeededAccessPreserved()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Register User A and User B
        var regAResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"user.a.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "User A"));
        Assert.Equal(HttpStatusCode.OK, regAResp.StatusCode);
        var regAJson = await regAResp.Content.ReadFromJsonAsync<JsonElement>();
        var tokenA = regAJson.GetProperty("accessToken").GetString()!;
        var userAId = regAJson.GetProperty("user").GetProperty("id").GetGuid();

        var regBResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"user.b.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "User B"));
        Assert.Equal(HttpStatusCode.OK, regBResp.StatusCode);
        var regBJson = await regBResp.Content.ReadFromJsonAsync<JsonElement>();
        var tokenB = regBJson.GetProperty("accessToken").GetString()!;
        var userBId = regBJson.GetProperty("user").GetProperty("id").GetGuid();

        // 2. User A uploads a private PDF document
        var pdfBytes = await File.ReadAllBytesAsync(Path.Combine(GetRepoRoot(), "test-fixtures", "small-3p.pdf"));
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pdfBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "private-user-a.pdf");
        form.Add(new StringContent("Tài liệu mật của User A"), "title");

        using var uploadReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/documents/upload") { Content = form };
        uploadReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var uploadResp = await client.SendAsync(uploadReq);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var docAJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docAId = docAJson.GetProperty("id").GetGuid();

        // 3. User A creates a private notebook, annotation, and flashcard
        using var nbReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notebooks/by-source")
        {
            Content = JsonContent.Create(new AutoProvisionNotebookRequest(null, docAId, "Ghi chú mật của User A"))
        };
        nbReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var nbResp = await client.SendAsync(nbReq);
        Assert.Equal(HttpStatusCode.OK, nbResp.StatusCode);
        var nbAJson = await nbResp.Content.ReadFromJsonAsync<JsonElement>();
        var nbAId = nbAJson.GetProperty("id").GetGuid();

        using var annReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/annotations")
        {
            Content = JsonContent.Create(new CreateAnnotationRequest(
                null, null, docAId, 1, null, "Private quote A", null, null, 0, 15, "yellow", "Core", "Private note"))
        };
        annReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var annResp = await client.SendAsync(annReq);
        Assert.Equal(HttpStatusCode.OK, annResp.StatusCode);

        using var fcReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards")
        {
            Content = JsonContent.Create(new CreateFlashcardRequest(
                null, docAId, null, "cloze", "Private Doc A", "Private quote", "Front A", "Back A", new List<string> { "PrivateTagA" }))
        };
        fcReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var fcResp = await client.SendAsync(fcReq);
        Assert.Equal(HttpStatusCode.OK, fcResp.StatusCode);

        // 4. User B attempts to read/modify User A's private resources via Bearer token & X-Test-User-Id
        static bool IsForbiddenOrNotFound(HttpStatusCode code) =>
            code is HttpStatusCode.Forbidden or HttpStatusCode.NotFound;

        // 4a. GET /api/v1/notebooks/{nbA.Id}
        using var bGetNb = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/notebooks/{nbAId}");
        bGetNb.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var bGetNbResp = await client.SendAsync(bGetNb);
        Assert.True(IsForbiddenOrNotFound(bGetNbResp.StatusCode), $"Expected 403/404 for GET notebook, got {bGetNbResp.StatusCode}");

        // 4b. PUT /api/v1/notebooks/{nbA.Id}
        using var bPutNb = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/notebooks/{nbAId}")
        {
            Content = JsonContent.Create(new SaveNotebookRequest(Guid.NewGuid(), 1, "attacker", "Hacked", "{\"type\":\"doc\",\"content\":[]}"))
        };
        bPutNb.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var bPutNbResp = await client.SendAsync(bPutNb);
        Assert.True(IsForbiddenOrNotFound(bPutNbResp.StatusCode), $"Expected 403/404 for PUT notebook, got {bPutNbResp.StatusCode}");

        // 4c. GET /api/v1/notebooks/{nbA.Id}/export-markdown
        using var bExportNb = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/notebooks/{nbAId}/export-markdown");
        bExportNb.Headers.Add("X-Test-User-Id", userBId.ToString());
        var bExportNbResp = await client.SendAsync(bExportNb);
        Assert.True(IsForbiddenOrNotFound(bExportNbResp.StatusCode), $"Expected 403/404 for export-markdown, got {bExportNbResp.StatusCode}");

        // 4d. GET /api/v1/documents/{docA.Id}
        using var bGetDoc = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docAId}");
        bGetDoc.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var bGetDocResp = await client.SendAsync(bGetDoc);
        Assert.True(IsForbiddenOrNotFound(bGetDocResp.StatusCode), $"Expected 403/404 for GET document, got {bGetDocResp.StatusCode}");

        // 4e. GET /api/v1/documents/{docA.Id}/file
        using var bGetDocFile = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docAId}/file");
        bGetDocFile.Headers.Add("X-Test-User-Id", userBId.ToString());
        var bGetDocFileResp = await client.SendAsync(bGetDocFile);
        Assert.True(IsForbiddenOrNotFound(bGetDocFileResp.StatusCode), $"Expected 403/404 for GET document file, got {bGetDocFileResp.StatusCode}");

        // 4f. GET /api/v1/practice-quizzes/{DefaultQuizId} as User B
        using var bGetQuiz = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/practice-quizzes/{SeedDataInitializer.DefaultQuizId}");
        bGetQuiz.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var bGetQuizResp = await client.SendAsync(bGetQuiz);
        Assert.True(IsForbiddenOrNotFound(bGetQuizResp.StatusCode), $"Expected 403/404 for GET foreign practice quiz, got {bGetQuizResp.StatusCode}");

        // 5. Verify DefaultUserId (single-user E2E default) still accesses seeded entities 100% green
        var defaultNbResp = await client.GetAsync($"/api/v1/notebooks/{SeedDataInitializer.DefaultNotebookId}");
        Assert.Equal(HttpStatusCode.OK, defaultNbResp.StatusCode);

        var defaultDocResp = await client.GetAsync($"/api/v1/documents/{SeedDataInitializer.SampleDocumentId}");
        Assert.Equal(HttpStatusCode.OK, defaultDocResp.StatusCode);

        var defaultQuizResp = await client.GetAsync($"/api/v1/practice-quizzes/{SeedDataInitializer.DefaultQuizId}");
        Assert.Equal(HttpStatusCode.OK, defaultQuizResp.StatusCode);
    }

    [Fact]
    public async Task Test2_ConcurrentNotebookOcc_15ParallelRequests_1SucceedsAnd14ConflictKeepBoth()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"occ.user.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "OCC Tester"));
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regJson.GetProperty("user").GetProperty("id").GetGuid();

        using var provReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notebooks/by-source")
        {
            Content = JsonContent.Create(new AutoProvisionNotebookRequest(SeedDataInitializer.LessonPagingId, null, "Bài 3.2"))
        };
        provReq.Headers.Add("X-Test-User-Id", userId.ToString());
        var provResp = await client.SendAsync(provReq);
        Assert.Equal(HttpStatusCode.OK, provResp.StatusCode);
        var nbJson = await provResp.Content.ReadFromJsonAsync<JsonElement>();
        var nbId = nbJson.GetProperty("id").GetGuid();
        Assert.Equal(1, nbJson.GetProperty("version").GetInt32());

        // Launch 15 parallel PUT requests all with BaseVersion = 1
        var tasks = Enumerable.Range(1, 15).Select(async i =>
        {
            var saveReq = new SaveNotebookRequest(
                Guid.NewGuid(),
                BaseVersion: 1,
                DeviceId: $"Device-{i}",
                Title: $"Ghi chú song song #{i}",
                ContentAstJson: $"{{\"type\":\"doc\",\"content\":[{{\"type\":\"paragraph\",\"content\":[{{\"type\":\"text\",\"text\":\"Writer {i}\"}}]}}]}}"
            );
            using var req = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/notebooks/{nbId}")
            {
                Content = JsonContent.Create(saveReq)
            };
            req.Headers.Add("X-Test-User-Id", userId.ToString());
            return await client.SendAsync(req);
        });

        var responses = await Task.WhenAll(tasks);

        var okCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, okCount);
        Assert.Equal(14, conflictCount);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

        var updatedNb = await db.Notebooks.AsNoTracking().FirstAsync(n => n.Id == nbId);
        Assert.Equal(2, updatedNb.Version);

        var conflictDraftsCount = await db.NotebookConflictDrafts.CountAsync(c => c.NotebookId == nbId);
        Assert.Equal(14, conflictDraftsCount);

        var userNotebooksCount = await db.Notebooks.CountAsync(n => n.UserId == userId);
        Assert.Equal(15, userNotebooksCount); // 1 canonical + 14 conflict forks
    }

    [Fact]
    public async Task Test3_ConcurrentNotebookAutoProvisioning_10ParallelRequests_ReturnsSameNotebookId()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"autoprov.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "AutoProv Tester"));
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regJson.GetProperty("user").GetProperty("id").GetGuid();

        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notebooks/by-source")
            {
                Content = JsonContent.Create(new AutoProvisionNotebookRequest(SeedDataInitializer.LessonPagingId, null, "Bài 3.2"))
            };
            req.Headers.Add("X-Test-User-Id", userId.ToString());
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("id").GetGuid();
        });

        var notebookIds = await Task.WhenAll(tasks);
        Assert.Single(notebookIds.Distinct());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
        var countInDb = await db.Notebooks.CountAsync(n => n.UserId == userId && n.PrimarySourceLessonId == SeedDataInitializer.LessonPagingId);
        Assert.Equal(1, countInDb);
    }

    [Fact]
    public async Task Test4_ConcurrentFsrsReviewIdempotency_20ParallelRequestsWithSameEventId_InsertsOneRowAndIncrementsRepsByOne()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"fsrs.idem.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "FSRS Idempotency Tester"));
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regJson.GetProperty("user").GetProperty("id").GetGuid();

        using var createCardReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards")
        {
            Content = JsonContent.Create(new CreateFlashcardRequest(
                SeedDataInitializer.LessonPagingId, null, null, "cloze", "Bài 3.2", "TLB quote", "Front {{c1::TLB}}", "Back TLB", null))
        };
        createCardReq.Headers.Add("X-Test-User-Id", userId.ToString());
        var createCardResp = await client.SendAsync(createCardReq);
        Assert.Equal(HttpStatusCode.OK, createCardResp.StatusCode);
        var cardJson = await createCardResp.Content.ReadFromJsonAsync<JsonElement>();
        var cardId = cardJson.GetProperty("id").GetGuid();

        var sharedReviewEventId = Guid.NewGuid();
        var clientTime = DateTimeOffset.UtcNow;

        var tasks = Enumerable.Range(1, 20).Select(async _ =>
        {
            var batch = new SyncReviewsBatchRequest(
            [
                new FsrsReviewEventInput(sharedReviewEventId, cardId, 3, clientTime, 2200)
            ]);
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards/sync-reviews")
            {
                Content = JsonContent.Create(batch)
            };
            req.Headers.Add("X-Test-User-Id", userId.ToString());
            return await client.SendAsync(req);
        });

        var responses = await Task.WhenAll(tasks);
        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

        var reviewRowCount = await db.FlashcardReviews.CountAsync(r => r.ReviewEventId == sharedReviewEventId);
        Assert.Equal(1, reviewRowCount);

        var updatedCard = await db.Flashcards.AsNoTracking().FirstAsync(f => f.Id == cardId);
        Assert.Equal(1, updatedCard.Reps);
    }

    [Fact]
    public async Task Test5_ConcurrentAiQuotaCeiling_20ParallelRequestsWith5RemainingCredits_5OkAnd15TooManyRequests()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"ai.quota.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "AI Quota Tester"));
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regJson.GetProperty("user").GetProperty("id").GetGuid();

        // Pre-seed 45 used credits out of 50 daily limit (leaving 5 remaining credits)
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SwmsDbContext>();
            db.AiUsageLedgers.Add(new AiUsageLedger
            {
                UserId = userId,
                Feature = "explain_concept",
                CreditsCharged = 45,
                Model = "gemini-3.8-flash",
                PromptTokens = 1000,
                CompletionTokens = 1000,
                TotalTokens = 2000,
                CostUsd = 0.001m,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var tasks = Enumerable.Range(1, 20).Select(async i =>
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/explain")
            {
                Content = JsonContent.Create(new AiInSituRequest($"Khái niệm TLB #{i}", "Bài 3.2", "explain"))
            };
            req.Headers.Add("X-Test-User-Id", userId.ToString());
            return await client.SendAsync(req);
        });

        var responses = await Task.WhenAll(tasks);

        var okCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        Assert.Equal(5, okCount);
        Assert.Equal(15, rateLimitedCount);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SwmsDbContext>();
        var totalUsed = await verifyDb.AiUsageLedgers
            .Where(l => l.UserId == userId && l.CreatedAt >= DateTimeOffset.UtcNow.Date)
            .SumAsync(l => l.CreditsCharged);
        Assert.Equal(50, totalUsed);
    }

    [Fact]
    public async Task Test6_ConcurrentRegistrationDuplicateEmail_10ParallelRequests_1OkAnd9Conflict()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var duplicateEmail = $"race.dup.{Guid.NewGuid():N}@studxy.vn";

        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            return await client.PostAsJsonAsync("/api/v1/auth/register",
                new RegisterRequest(duplicateEmail, "Studxy@2026", $"Duplicate Runner {i}"));
        });

        var responses = await Task.WhenAll(tasks);

        var okCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, okCount);
        Assert.Equal(9, conflictCount);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
        var userCount = await db.Users.CountAsync(u => u.Email == duplicateEmail);
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task Test7_StandaloneNotebookCreation_And_UndoLastFlashcardReview_Succeed()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"feature7.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Feature 7 Tester"));
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = regJson.GetProperty("user").GetProperty("id").GetGuid();

        // Part A: Standalone Notebook Creation via POST /api/v1/notebooks
        using var createNbReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notebooks")
        {
            Content = JsonContent.Create(new { title = "Sổ tay tổng hợp độc lập", contentAstJson = (string?)null })
        };
        createNbReq.Headers.Add("X-Test-User-Id", userId.ToString());
        var createNbResp = await client.SendAsync(createNbReq);
        Assert.Equal(HttpStatusCode.OK, createNbResp.StatusCode);

        var nbBody = await createNbResp.Content.ReadFromJsonAsync<JsonElement>();
        var standaloneNbId = nbBody.GetProperty("id").GetGuid();
        Assert.Equal("Sổ tay tổng hợp độc lập", nbBody.GetProperty("title").GetString());
        Assert.Equal(1, nbBody.GetProperty("version").GetInt32());
        Assert.Equal(JsonValueKind.Null, nbBody.GetProperty("primarySourceLessonId").ValueKind);
        Assert.Equal(JsonValueKind.Null, nbBody.GetProperty("primarySourceDocumentId").ValueKind);

        // Part B: Undo Last Flashcard Review via POST /api/v1/flashcards/{id}/undo-last-review
        using var createCardReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards")
        {
            Content = JsonContent.Create(new CreateFlashcardRequest(
                SeedDataInitializer.LessonPagingId, null, null, "cloze", "Bài 3.2", "Quote", "Front {{c1::Undo}}", "Back Undo", null))
        };
        createCardReq.Headers.Add("X-Test-User-Id", userId.ToString());
        var createCardResp = await client.SendAsync(createCardReq);
        Assert.Equal(HttpStatusCode.OK, createCardResp.StatusCode);
        var cardBody = await createCardResp.Content.ReadFromJsonAsync<JsonElement>();
        var cardId = cardBody.GetProperty("id").GetGuid();

        // Sync 1 review event (Rating = 3 Good)
        var reviewEventId = Guid.NewGuid();
        using var syncReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/flashcards/sync-reviews")
        {
            Content = JsonContent.Create(new SyncReviewsBatchRequest(
            [
                new FsrsReviewEventInput(reviewEventId, cardId, 3, DateTimeOffset.UtcNow, 1800)
            ]))
        };
        syncReq.Headers.Add("X-Test-User-Id", userId.ToString());
        var syncResp = await client.SendAsync(syncReq);
        Assert.Equal(HttpStatusCode.OK, syncResp.StatusCode);

        // Undo last review
        var beforeUndo = DateTimeOffset.UtcNow.AddSeconds(-2);
        using var undoReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/flashcards/{cardId}/undo-last-review");
        undoReq.Headers.Add("X-Test-User-Id", userId.ToString());
        var undoResp = await client.SendAsync(undoReq);
        Assert.Equal(HttpStatusCode.OK, undoResp.StatusCode);

        var undoBody = await undoResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(cardId, undoBody.GetProperty("flashcardId").GetGuid());
        Assert.Equal(0, undoBody.GetProperty("reps").GetInt32());
        Assert.Equal(0.0, undoBody.GetProperty("stability").GetDouble(), 4);
        Assert.Equal(0.0, undoBody.GetProperty("difficulty").GetDouble(), 4);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
        var revertedCard = await db.Flashcards.AsNoTracking().FirstAsync(f => f.Id == cardId);
        Assert.Equal(0, revertedCard.Reps);
        Assert.Equal(0.0, revertedCard.Stability, 4);
        Assert.Equal(0.0, revertedCard.Difficulty, 4);
        Assert.True(revertedCard.DueAt >= beforeUndo && revertedCard.DueAt <= DateTimeOffset.UtcNow.AddSeconds(5));
        Assert.Equal(0, await db.FlashcardReviews.CountAsync(r => r.FlashcardId == cardId));
    }
}
