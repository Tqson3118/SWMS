using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using OrderManagement;
using Xunit;

public sealed class IdempotencyTests
{
    [Fact]
    public void Requires_exactly_one_header_value()
    {
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.NormalizeIdempotencyKey(Array.Empty<string?>()));
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.NormalizeIdempotencyKey(new string?[] { "A", "B" }));
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.NormalizeIdempotencyKey(new string?[] { null }));
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.NormalizeIdempotencyKey(new string?[] { "" }));
    }

    [Fact]
    public void Accepts_http_token_punctuation_and_normalizes_invariant_case()
    {
        const string token = "!#$%&'*+-.^_`|~Az09";
        Assert.Equal(token.ToUpperInvariant(), ExpenseIdempotencyService.NormalizeIdempotencyKey(new[] { token }));
    }

    [Theory]
    [InlineData("has space")]
    [InlineData("has\ttab")]
    [InlineData("has,comma")]
    [InlineData("has\nline")]
    [InlineData("é")]
    [InlineData("a:b")]
    public void Rejects_non_http_token_headers(string value) =>
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.NormalizeIdempotencyKey(new[] { value }));

    [Fact]
    public void Enforces_128_character_boundary()
    {
        var valid = new string('a', 128);
        Assert.Equal(valid.ToUpperInvariant(), ExpenseIdempotencyService.NormalizeIdempotencyKey(new[] { valid }));
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.NormalizeIdempotencyKey(new[] { valid + "a" }));
    }

    [Fact]
    public void Header_normalization_is_invariant_under_turkish_culture()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.Equal("I", ExpenseIdempotencyService.NormalizeIdempotencyKey(new[] { "i" }));
    }

    [Fact]
    public void Known_vector_uses_compact_versioned_json_and_uppercase_sha256()
    {
        Assert.Equal("1080342A0382306BBF8BA3E70BA04C1AE204810CAF8F4B84D0A9739A518BFC2A", ExpenseIdempotencyService.ComputeRequestHash(12.30m, "note"));
    }

    [Fact]
    public void Equivalent_valid_decimal_representations_hash_equally_and_culture_does_not_change_them()
    {
        using var culture = new CultureScope("tr-TR");
        Assert.Equal(ExpenseIdempotencyService.ComputeRequestHash(1m, "n"), ExpenseIdempotencyService.ComputeRequestHash(1.00m, "n"));
    }

    [Fact]
    public void Preserves_unicode_whitespace_and_escaped_note_content_without_normalization()
    {
        var note = "  café\nquote=\"x\" slash=\\ emoji=🙂  ";
        var same = ExpenseIdempotencyService.ComputeRequestHash(1m, note);
        Assert.Equal(64, same.Length);
        Assert.NotEqual(same, ExpenseIdempotencyService.ComputeRequestHash(1m, note.Normalize(NormalizationForm.FormD)));
        Assert.NotEqual(same, ExpenseIdempotencyService.ComputeRequestHash(1m, note.Trim()));
    }

    [Fact]
    public void Rejects_invalid_amount_precision_range_and_note_size()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(-0.01m, "n"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(1.001m, "n"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(10000000000000000.00m, "n"));
        Assert.Throws<ArgumentNullException>(() => ExpenseIdempotencyService.ComputeRequestHash(1m, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(1m, new string('x', 1001)));
    }

    [Fact]
    public void Unicode_escape_vector_matches_independently_specified_utf8_json_bytes()
    {
        const string note = "  café\nquote=\"x\" slash=\\ emoji=🙂  ";
        Assert.Equal("C91D881D3FA5797A34B9A57D256D4570428CFFF563AB2B2B473B6BDC28D5C5D9", ExpenseIdempotencyService.ComputeRequestHash(12.34m, note));
    }

    [Fact]
    public void Amount_range_matches_existing_expense_endpoint()
    {
        Assert.Equal(64, ExpenseIdempotencyService.ComputeRequestHash(0.01m, "n").Length);
        Assert.Equal(64, ExpenseIdempotencyService.ComputeRequestHash(1_000_000m, "n").Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(0m, "n"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(1_000_000.01m, "n"));
    }

    [Fact]
    public void Note_boundary_accepts_empty_and_exact_500_but_rejects_501()
    {
        Assert.Equal(64, ExpenseIdempotencyService.ComputeRequestHash(1m, "").Length);
        Assert.Equal(64, ExpenseIdempotencyService.ComputeRequestHash(1m, new string('x', 500)).Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => ExpenseIdempotencyService.ComputeRequestHash(1m, new string('x', 501)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Malformed_utf16_notes_are_rejected_instead_of_replaced_during_json_encoding(int variant)
    {
        var note = variant switch { 0 => "\uD800", 1 => "\uDC00", 2 => "x\uD800y", 3 => "\uDC00\uD800", _ => throw new ArgumentOutOfRangeException(nameof(variant)) };
        Assert.Throws<ArgumentException>(() => ExpenseIdempotencyService.ComputeRequestHash(1m, note));
    }

    [Fact]
    public void Valid_surrogate_pair_and_replacement_character_remain_distinct_valid_notes()
    {
        var supplementary = ExpenseIdempotencyService.ComputeRequestHash(1m, "\uD83D\uDE42");
        var replacement = ExpenseIdempotencyService.ComputeRequestHash(1m, "\uFFFD");
        Assert.Equal(64, supplementary.Length);
        Assert.Equal(64, replacement.Length);
        Assert.NotEqual(supplementary, replacement);
    }

    [Fact]
    public void C1b_exposes_a_durable_create_or_replay_operation()
    {
        var method = typeof(ExpenseIdempotencyService).GetMethod("CreateOrReplayAsync");
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<IdempotencyOperationResult>), method!.ReturnType);
    }

    [Fact]
    public void Receipt_fk_uses_the_tracked_temporary_expense_key_before_receipt_is_added()
    {
        var options = new DbContextOptionsBuilder<AuthDb>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=tracker-only;Integrated Security=True")
            .Options;
        using var db = new AuthDb(options);
        var expense = new Expense { OwnerUserId = "owner", Amount = 1m, Note = "n" };
        db.Expenses.Add(expense);
        var trackedTemporaryId = db.Entry(expense).Property(x => x.Id).CurrentValue;
        var receipt = new ExpenseIdempotencyReceipt { OwnerUserId = "owner", NormalizedKey = "K", RequestHash = new string('A', 64), ExpenseId = trackedTemporaryId, OriginalAmount = 1m, OriginalNote = "n" };
        db.ExpenseIdempotencyReceipts.Add(receipt);
        Assert.True(db.Entry(expense).Property(x => x.Id).IsTemporary);
        Assert.Equal(trackedTemporaryId, receipt.ExpenseId);
    }

    [Fact]
    public void Duplicate_classifier_requires_exact_sql_code_and_unique_index_token()
    {
        const string valid = "Cannot insert duplicate key row with unique index 'UX_ExpenseIdempotencyReceipts_Owner_Key'.";
        Assert.True(ExpenseIdempotencyService.IsIntendedReceiptDuplicate(2601, valid));
        Assert.True(ExpenseIdempotencyService.IsIntendedReceiptDuplicate(2627, valid));
        Assert.False(ExpenseIdempotencyService.IsIntendedReceiptDuplicate(547, valid));
        Assert.False(ExpenseIdempotencyService.IsIntendedReceiptDuplicate(2601, valid.Replace("UX_ExpenseIdempotencyReceipts_Owner_Key", "UX_Other")));
        Assert.False(ExpenseIdempotencyService.IsIntendedReceiptDuplicate(2601, "duplicate value UX_ExpenseIdempotencyReceipts_Owner_Key without unique index token"));
    }

    [Fact]
    public async Task Cancellation_before_dispatch_propagates_without_creating_a_scope()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var scopes = new ThrowingScopeFactory();
        var service = new ExpenseIdempotencyService(scopes, NullLogger<ExpenseIdempotencyService>.Instance);
        await Assert.ThrowsAsync<OperationCanceledException>(() => service.CreateOrReplayAsync("owner", new[] { "cancel-key" }, 1m, "note", cancellation.Token));
        Assert.Equal(0, scopes.CreateCalls);
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo old = CultureInfo.CurrentCulture;
        private readonly CultureInfo oldUi = CultureInfo.CurrentUICulture;
        public CultureScope(string name) { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name); CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name); }
        public void Dispose() { CultureInfo.CurrentCulture = old; CultureInfo.CurrentUICulture = oldUi; }
    }
    private sealed class ThrowingScopeFactory : IServiceScopeFactory
    {
        public int CreateCalls { get; private set; }
        public IServiceScope CreateScope() { CreateCalls++; throw new InvalidOperationException("scope must not be created"); }
    }
}

public sealed class IdempotencyRuntimeTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private HttpClient clientB = null!;
    private string password = $"C1b-{Guid.NewGuid():N}!";
    private string? previousDatabase;
    private readonly SaveFaults faults = new();
    private readonly RaceBarrier raceBarrier = new();
    private readonly CancelAtProviderCommand cancelAtSave = new();
    private readonly CapturedLogs logs = new();

    public async Task InitializeAsync()
    {
        previousDatabase = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", $"RoadmapAuth11_{Guid.NewGuid():N}");
        try
        {
            factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(logs);
                    logging.AddFilter((category, _) => string.Equals(category, "RoadmapAuth11.SafeErrors", StringComparison.Ordinal)
                        || string.Equals(category, "OrderManagement.ExpenseIdempotencyService", StringComparison.Ordinal));
                });
                builder.ConfigureServices(services => services.AddDbContext<AuthDb>((_, options) => options.AddInterceptors(faults, raceBarrier, cancelAtSave)));
            });
            client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            clientB = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            client.BaseAddress = new Uri("https://localhost");
            clientB.BaseAddress = client.BaseAddress;
            await Register(client, $"c1b-{Guid.NewGuid():N}@example.test", password);
            await Register(clientB, $"c1b-{Guid.NewGuid():N}@example.test", $"C1b-{Guid.NewGuid():N}!");
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal(new[] { "20261009001000_InitialStage11IdentityExpense", "20261009002000_AddExpenseIdempotencyReceipts" }, (await db.Database.GetAppliedMigrationsAsync()).ToArray());
        }
        catch
        {
            client?.Dispose();
            clientB?.Dispose();
            factory?.Dispose();
            Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", previousDatabase);
            throw;
        }
    }

    private static async Task Register(HttpClient target, string email, string password)
    {
        var csrf = await target.GetFromJsonAsync<Csrf>("/api/csrf");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/register") { Content = JsonContent.Create(new { email, password }) };
        request.Headers.Add("X-CSRF-TOKEN", csrf!.Token);
        Assert.Equal(HttpStatusCode.OK, (await target.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Create_replay_conflict_and_put_keep_original_receipt_immutable()
    {
        var csrf = await client.GetFromJsonAsync<Csrf>("/api/csrf");
        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 12.34m, note = "original", ownerUserId = "ignored" }) };
        first.Headers.Add("X-CSRF-TOKEN", csrf!.Token); first.Headers.Add("Idempotency-Key", "receipt-c1b");
        using var created = await client.SendAsync(first);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("/api/expenses/1", created.Headers.Location?.ToString());
        var original = await created.Content.ReadFromJsonAsync<ReceiptBody>();

        using var replay = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 12.34m, note = "original", ownerUserId = "ignored" }) };
        replay.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); replay.Headers.Add("Idempotency-Key", "RECEIPT-C1B");
        using var replayed = await client.SendAsync(replay);
        Assert.Equal(HttpStatusCode.Created, replayed.StatusCode);
        Assert.Equal(created.Headers.Location, replayed.Headers.Location);
        Assert.Equal(await created.Content.ReadAsStringAsync(), await replayed.Content.ReadAsStringAsync());

        using var changed = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 99.99m, note = "changed", ownerUserId = "ignored" }) };
        changed.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); changed.Headers.Add("Idempotency-Key", "receipt-c1b");
        Assert.Equal(HttpStatusCode.Conflict, (await client.SendAsync(changed)).StatusCode);

        using var put = new HttpRequestMessage(HttpMethod.Put, $"/api/expenses/{original!.Id}") { Content = JsonContent.Create(new { amount = 20.00m, note = "updated", ownerUserId = "ignored" }) };
        put.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(put)).StatusCode);
        using var afterPut = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 12.34m, note = "original", ownerUserId = "ignored" }) };
        afterPut.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); afterPut.Headers.Add("Idempotency-Key", "receipt-c1b");
        var immutable = await (await client.SendAsync(afterPut)).Content.ReadFromJsonAsync<ReceiptBody>();
        Assert.Equal(original, immutable);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
            Assert.Equal(1, await db.Expenses.CountAsync());
            Assert.Equal(1, await db.ExpenseIdempotencyReceipts.CountAsync());
        }

        faults.ThrowAfterCommit = true;
        var faultToken = (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token;
        using var faultRequest = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 33.33m, note = "ack marker" }) };
        faultRequest.Headers.Add("X-CSRF-TOKEN", faultToken); faultRequest.Headers.Add("Idempotency-Key", "after-commit-ack");
        using var faultResponse = await client.SendAsync(faultRequest);
        Assert.Equal(HttpStatusCode.InternalServerError, faultResponse.StatusCode);
        var faultText = await faultResponse.Content.ReadAsStringAsync();
        var faultBody = JsonSerializer.Deserialize<ErrorBody>(faultText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(faultBody);
        Assert.DoesNotContain("ack marker", faultText, StringComparison.Ordinal);
        Assert.Contains(logs.Entries, entry => entry.Contains("auth_server_error", StringComparison.Ordinal) && entry.Contains(faultBody!.TraceId, StringComparison.Ordinal));
        faults.ThrowAfterCommit = false;
        using var retry = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 33.33m, note = "ack marker" }) };
        retry.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); retry.Headers.Add("Idempotency-Key", "after-commit-ack");
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(retry)).StatusCode);
        var captured = string.Join('\n', logs.Entries);
        Assert.DoesNotContain("ack marker", captured, StringComparison.Ordinal);
        Assert.DoesNotContain("RequestHash", captured, StringComparison.Ordinal);
        Assert.DoesNotContain("bounded", captured, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StackTrace", captured, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine(string.Join(" | ", logs.Entries.Where(entry => entry.Contains("auth_server_error", StringComparison.Ordinal))));
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
            Assert.Equal(2, await db.Expenses.CountAsync());
            Assert.Equal(2, await db.ExpenseIdempotencyReceipts.CountAsync());
        }
    }

    [Fact]
    public async Task Same_key_is_owner_scoped_and_concurrent_same_owner_requests_share_one_receipt()
    {
        var aToken = (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token;
        var bToken = (await clientB.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token;
        using var bRequest = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 7.00m, note = "other owner" }) };
        bRequest.Headers.Add("X-CSRF-TOKEN", bToken); bRequest.Headers.Add("Idempotency-Key", "race-key");
        using var bCreated = await clientB.SendAsync(bRequest);
        Assert.Equal(HttpStatusCode.Created, bCreated.StatusCode);
        var bBody = await bCreated.Content.ReadFromJsonAsync<ReceiptBody>();
        Assert.NotNull(bBody);

        async Task<HttpResponseMessage> SendRace()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 8.00m, note = "race" }) };
            request.Headers.Add("X-CSRF-TOKEN", aToken); request.Headers.Add("Idempotency-Key", "race-key");
            return await client.SendAsync(request);
        }
        raceBarrier.Arm();
        var responses = await Task.WhenAll(SendRace(), SendRace());
        raceBarrier.Disarm();
        using var first = responses[0];
        using var second = responses[1];
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(first.Headers.Location, second.Headers.Location);
        var aBody = await first.Content.ReadFromJsonAsync<ReceiptBody>();
        var aReplayBody = await second.Content.ReadFromJsonAsync<ReceiptBody>();
        Assert.NotNull(aBody);
        Assert.NotEqual(bBody!.Id, aBody!.Id);
        Assert.Equal(aBody, aReplayBody);
        using var bReplay = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 7.00m, note = "other owner" }) };
        bReplay.Headers.Add("X-CSRF-TOKEN", (await clientB.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); bReplay.Headers.Add("Idempotency-Key", "race-key");
        var bReplayBody = await (await clientB.SendAsync(bReplay)).Content.ReadFromJsonAsync<ReceiptBody>();
        Assert.Equal(bBody, bReplayBody);
        using var aReplay = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 8.00m, note = "race" }) };
        aReplay.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); aReplay.Headers.Add("Idempotency-Key", "race-key");
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(aReplay)).StatusCode);
        using var aChanged = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 8.01m, note = "changed A" }) };
        aChanged.Headers.Add("X-CSRF-TOKEN", (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); aChanged.Headers.Add("Idempotency-Key", "race-key");
        Assert.Equal(HttpStatusCode.Conflict, (await client.SendAsync(aChanged)).StatusCode);
        using var bChanged = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 7.01m, note = "changed B" }) };
        bChanged.Headers.Add("X-CSRF-TOKEN", (await clientB.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); bChanged.Headers.Add("Idempotency-Key", "race-key");
        Assert.Equal(HttpStatusCode.Conflict, (await clientB.SendAsync(bChanged)).StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
            Assert.Equal(2, await db.Expenses.CountAsync());
            Assert.Equal(2, await db.ExpenseIdempotencyReceipts.CountAsync());
        }
        Assert.Contains(logs.Entries, entry => entry.Contains("idempotency_duplicate_sql", StringComparison.Ordinal)
            && entry.Contains("number=", StringComparison.Ordinal)
            && entry.Contains("index=UX_ExpenseIdempotencyReceipts_Owner_Key", StringComparison.Ordinal));
        Console.WriteLine(string.Join(" | ", logs.Entries.Where(entry => entry.Contains("idempotency_duplicate_sql", StringComparison.Ordinal))));
    }

    [Fact]
    public async Task Missing_and_multiple_idempotency_headers_are_rejected()
    {
        var token = (await client.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token;
        var writesBefore = faults.SaveAttempts;
        using var missing = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 3.00m, note = "missing" }) };
        missing.Headers.Add("X-CSRF-TOKEN", token);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(missing)).StatusCode);
        using var multiple = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 3.00m, note = "multiple" }) };
        multiple.Headers.Add("X-CSRF-TOKEN", token); multiple.Headers.TryAddWithoutValidation("Idempotency-Key", new[] { "one", "two" });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(multiple)).StatusCode);
        using var invalid = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 3.00m, note = "invalid" }) };
        invalid.Headers.Add("X-CSRF-TOKEN", token); invalid.Headers.Add("Idempotency-Key", "bad key");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(invalid)).StatusCode);
        using var overlong = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 3.00m, note = "overlong" }) };
        overlong.Headers.Add("X-CSRF-TOKEN", token); overlong.Headers.Add("Idempotency-Key", new string('x', 129));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(overlong)).StatusCode);
        using var invalidBody = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 0m, note = "invalid body" }) };
        invalidBody.Headers.Add("X-CSRF-TOKEN", token); invalidBody.Headers.Add("Idempotency-Key", "valid-body-invalid");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(invalidBody)).StatusCode);
        Assert.Equal(writesBefore, faults.SaveAttempts);
    }

    [Fact]
    public async Task Cancellation_after_save_dispatch_is_reported_as_unknown_outcome()
    {
        using var cancellation = new CancellationTokenSource();
        cancelAtSave.TokenSource = cancellation;
        cancelAtSave.Enabled = true;
        await using var scope = factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<OrderManagement.ExpenseIdempotencyService>();
        await Assert.ThrowsAsync<OrderManagement.IdempotencyUnknownOutcomeException>(() =>
            service.CreateOrReplayAsync("dispatch-cancel-owner", new[] { "dispatch-cancel-key" }, 6m, "dispatch cancellation", cancellation.Token));
        cancelAtSave.Enabled = false;
    }

    [Fact]
    public async Task Receipt_insert_failure_after_expense_insert_rolls_back_both_rows()
    {
        var previous = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        var database = $"RoadmapAuth11_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", database);
        var fault = new ReceiptInsertFault();
        try
        {
            await using var rollbackFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddDbContext<AuthDb>((_, options) =>
                    options.UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=True;TrustServerCertificate=True;", sql => sql.MaxBatchSize(1)).AddInterceptors(fault))));
            using var rollbackClient = rollbackFactory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            rollbackClient.BaseAddress = new Uri("https://localhost");
            await Register(rollbackClient, $"rollback-{Guid.NewGuid():N}@example.test", $"Rollback-{Guid.NewGuid():N}!");
            fault.Enabled = true;
            var token = (await rollbackClient.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token;
            using var failedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 41.00m, note = "rollback marker" }) };
            failedRequest.Headers.Add("X-CSRF-TOKEN", token); failedRequest.Headers.Add("Idempotency-Key", "rollback-key");
            using var failed = await rollbackClient.SendAsync(failedRequest);
            Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
            Assert.True(fault.ExpenseInsertCompleted);
            Assert.True(fault.ReceiptInsertBlocked);
            Assert.True(fault.SameTransaction);
            await using (var scope = rollbackFactory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
                Assert.Equal(0, await db.Expenses.CountAsync());
                Assert.Equal(0, await db.ExpenseIdempotencyReceipts.CountAsync());
            }
            fault.Enabled = false;
            using var retry = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount = 41.00m, note = "rollback marker" }) };
            retry.Headers.Add("X-CSRF-TOKEN", (await rollbackClient.GetFromJsonAsync<Csrf>("/api/csrf"))!.Token); retry.Headers.Add("Idempotency-Key", "rollback-key");
            Assert.Equal(HttpStatusCode.Created, (await rollbackClient.SendAsync(retry)).StatusCode);
            await using (var scope = rollbackFactory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
                Assert.Equal(1, await db.Expenses.CountAsync());
                Assert.Equal(1, await db.ExpenseIdempotencyReceipts.CountAsync());
            }
        }
        finally { Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", previous); }
    }

    public Task DisposeAsync() { client?.Dispose(); clientB?.Dispose(); factory?.Dispose(); Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", previousDatabase); return Task.CompletedTask; }
    private sealed record Csrf(string Header, string Token);
    private sealed record ReceiptBody(int Id, string Amount, string Note);
    private sealed record ErrorBody(string Error, string TraceId);
    private sealed class SaveFaults : SaveChangesInterceptor
    {
        public bool ThrowAfterCommit { get; set; }
        private int saveAttempts;
        public int SaveAttempts => Volatile.Read(ref saveAttempts);
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref saveAttempts);
            return ValueTask.FromResult(result);
        }
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
            => ThrowAfterCommit ? throw new InvalidOperationException("after-commit acknowledgement failure") : ValueTask.FromResult(result);
    }
    private sealed class CancelAtProviderCommand : DbCommandInterceptor
    {
        public bool Enabled { get; set; }
        public CancellationTokenSource? TokenSource { get; set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (Enabled && command.CommandText.Contains("INSERT INTO [Expenses]", StringComparison.Ordinal)) TokenSource!.Cancel();
            return ValueTask.FromResult(result);
        }
    }
    private sealed class CapturedLogs : ILoggerProvider
    {
        public ConcurrentQueue<string> Entries { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CapturedLogger(categoryName, Entries);
        public void Dispose() { }
    }
    private sealed class CapturedLogger(string category, ConcurrentQueue<string> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => entries.Enqueue(category + ": " + formatter(state, exception));
    }
    private sealed class ReceiptInsertFault : DbCommandInterceptor
    {
        public bool Enabled { get; set; }
        public bool ExpenseInsertCompleted { get; private set; }
        public bool ReceiptInsertBlocked { get; private set; }
        public bool SameTransaction { get; private set; }
        private object? transaction;

        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (Enabled && command.CommandText.Contains("INSERT INTO [Expenses]", StringComparison.Ordinal))
            {
                ExpenseInsertCompleted = true;
                transaction = eventData.Command.Transaction;
            }
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (Enabled && ExpenseInsertCompleted && command.CommandText.Contains("INSERT INTO [ExpenseIdempotencyReceipts]", StringComparison.Ordinal))
            {
                ReceiptInsertBlocked = true;
                SameTransaction = ReferenceEquals(transaction, eventData.Command.Transaction);
                throw new OperationCanceledException();
            }
            return ValueTask.FromResult(result);
        }
    }
    private sealed class RaceBarrier : SaveChangesInterceptor
    {
        private int arrivals;
        private TaskCompletionSource<bool> release = NewRelease();
        public bool Enabled { get; private set; }
        public void Arm() { arrivals = 0; release = NewRelease(); Enabled = true; }
        public void Disarm() => Enabled = false;
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!Enabled) return result;
            if (Interlocked.Increment(ref arrivals) == 2) release.TrySetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            return result;
        }
        private static TaskCompletionSource<bool> NewRelease() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
