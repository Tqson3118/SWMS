using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit;

public sealed class ErrorPrivacyTests
{
    [Fact]
    public async Task Unexpected_pre_save_failure_is_safe_in_HTTP_logs_and_SQL()
    {
        const string marker = "fictional-sensitive-provider-marker";
        var previous = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", $"RoadmapAuth11_{Guid.NewGuid():N}");
        try
        {
            var fault = new SaveFault(marker);
            var captured = new CapturedLogs();
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging => { logging.ClearProviders(); logging.AddProvider(captured); });
                builder.ConfigureServices(services => services.AddDbContext<AuthDb>((_, options) => options.AddInterceptors(fault)));
            });
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            client.BaseAddress = new Uri("https://localhost");
            var token = await Token(client);
            using var register = await Send(client, "/api/register", new { email = "privacy@example.test", password = $"Fictional-{Guid.NewGuid():N}!" }, token);
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);
            token = await Token(client);
            captured.Entries.Clear();
            fault.Enabled = true;
            var expenseKey = $"c2-{Guid.NewGuid():N}";
            using var failed = await Send(client, "/api/expenses", new { amount = 10.25m, note = "fixture note" }, token, expenseKey);
            Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
            var body = await failed.Content.ReadAsStringAsync();
            Assert.DoesNotContain(marker, body, StringComparison.Ordinal);
            Assert.DoesNotContain("private-fixture-path", body, StringComparison.Ordinal);
            Assert.DoesNotContain(marker, string.Join('\n', captured.Entries), StringComparison.Ordinal);
            Assert.DoesNotContain("private-fixture-path", string.Join('\n', captured.Entries), StringComparison.Ordinal);
            var error = (await failed.Content.ReadFromJsonAsync<ErrorResponse>())!;
            Assert.Equal("server error", error.Error);
            Assert.False(string.IsNullOrWhiteSpace(error.TraceId));
            Assert.Contains(captured.Entries, entry => entry.Contains("auth_server_error", StringComparison.Ordinal)
                && entry.Contains(error.TraceId, StringComparison.Ordinal));
            using (var scope = factory.Services.CreateScope())
                Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AuthDb>().Expenses.CountAsync());
            fault.Enabled = false;
            using var positive = await Send(client, "/api/expenses", new { amount = 10.25m, note = "fixture note" }, token, expenseKey);
            Assert.Equal(HttpStatusCode.Created, positive.StatusCode);
            using (var scope = factory.Services.CreateScope())
            {
                var row = await scope.ServiceProvider.GetRequiredService<AuthDb>().Expenses.AsNoTracking().SingleAsync();
                Assert.Equal(10.25m, row.Amount);
                Assert.Equal("fixture note", row.Note);
            }
        }
        finally { Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", previous); }
    }
    private static async Task<string> Token(HttpClient client) => (await client.GetFromJsonAsync<TokenResponse>("/api/csrf"))!.Token;
    private static async Task<HttpResponseMessage> Send(HttpClient client, string path, object body, string token, string? idempotencyKey = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", token);
        if (path == "/api/expenses") request.Headers.Add("Idempotency-Key", idempotencyKey ?? $"c2-{Guid.NewGuid():N}");
        return await client.SendAsync(request);
    }
    private sealed record TokenResponse(string Header, string Token);
    private sealed record ErrorResponse(string Error, string TraceId);
    private sealed class SaveFault(string marker) : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Enabled) throw new InvalidOperationException(marker + ": D:/private-fixture-path/account.txt");
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
            => entries.Enqueue(category + ": " + formatter(state, exception) + (exception is null ? "" : " | " + exception));
    }
}
