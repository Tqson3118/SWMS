using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

public sealed class StorageCsrfTests
{
    [Fact]
    public Task Ownership_and_rejected_updates_are_verified_in_SQL() => WithFixture(async (factory, a, b, ownerA, ownerB) =>
    {
        var tokenA = await a.Token();
        using var noToken = await a.Send(HttpMethod.Post, "/api/expenses", new { amount = 12.34m, note = "A" });
        Assert.Equal(HttpStatusCode.BadRequest, noToken.StatusCode);
        Assert.Empty(await Rows(factory));
        using var created = await a.Send(HttpMethod.Post, "/api/expenses", new { amount = 12.34m, note = "A", ownerUserId = ownerB }, tokenA);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<RowResponse>())!;
        Assert.Equal("12.34", item.Amount);
        Assert.Equal(new[] { new StoredRow(item.Id, ownerA, 12.34m, "A") }, await Rows(factory));
        using var foreignGet = await b.Send(HttpMethod.Get, $"/api/expenses/{item.Id}");
        Assert.Equal(HttpStatusCode.NotFound, foreignGet.StatusCode);
        using var foreignPut = await b.Send(HttpMethod.Put, $"/api/expenses/{item.Id}", new { amount = 99.99m, note = "B takeover", ownerUserId = ownerA }, await b.Token());
        Assert.Equal(HttpStatusCode.NotFound, foreignPut.StatusCode);
        Assert.Equal(new[] { new StoredRow(item.Id, ownerA, 12.34m, "A") }, await Rows(factory));
        using var missingPutToken = await a.Send(HttpMethod.Put, $"/api/expenses/{item.Id}", new { amount = 99.99m, note = "missing token" });
        Assert.Equal(HttpStatusCode.BadRequest, missingPutToken.StatusCode);
        Assert.Equal(new[] { new StoredRow(item.Id, ownerA, 12.34m, "A") }, await Rows(factory));
        using var ownPut = await a.Send(HttpMethod.Put, $"/api/expenses/{item.Id}", new { amount = 13.25m, note = "A updated", ownerUserId = ownerB }, tokenA);
        Assert.Equal(HttpStatusCode.OK, ownPut.StatusCode);
        var updated = (await ownPut.Content.ReadFromJsonAsync<RowResponse>())!;
        Assert.Equal("13.25", updated.Amount);
        Assert.Equal("A updated", updated.Note);
        Assert.Equal(new[] { new StoredRow(item.Id, ownerA, 13.25m, "A updated") }, await Rows(factory));
        using var ownGet = await a.Send(HttpMethod.Get, $"/api/expenses/{item.Id}");
        Assert.Equal(updated, await ownGet.Content.ReadFromJsonAsync<RowResponse>());
    });

    [Fact]
    public Task Invalid_money_note_and_null_shapes_never_mutate_the_fixture() => WithFixture(async (factory, a, b, ownerA, ownerB) =>
    {
        var token = await a.Token();
        using var created = await a.Send(HttpMethod.Post, "/api/expenses", new { amount = 1_000_000m, note = new string('n', 500) }, token);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<RowResponse>())!;
        Assert.Equal("1000000.00", item.Amount);
        var before = await Rows(factory);
        var invalid = new object?[] {
            new { amount = 12.345m, note = "fractional" },
            new { amount = 1_000_000.01m, note = "above cap" },
            new { amount = 0m, note = "zero" },
            new { amount = -1m, note = "negative" },
            new { amount = (decimal?)null, note = "missing amount" },
            new { amount = 1m, note = (string?)null },
            new { amount = 1m, note = new string('x', 501) },
            null
        };
        foreach (var body in invalid)
        {
            foreach (var method in new[] { HttpMethod.Post, HttpMethod.Put })
            {
                var path = method == HttpMethod.Post ? "/api/expenses" : $"/api/expenses/{item.Id}";
                using var rejected = await a.Send(method, path, body, token, hasBody: true);
                Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
                Assert.Equal(before, await Rows(factory));
            }
        }
    });

    [Fact]
    public Task Matched_foreign_CSRF_pair_cannot_act_with_another_users_auth_cookie() => WithFixture(async (factory, a, b, ownerA, ownerB) =>
    {
        var tokenA = await a.Token();
        var tokenB = await b.Token();
        var antiName = factory.Services.GetRequiredService<IOptions<AntiforgeryOptions>>().Value.Cookie.Name!;
        var foreignAntiCookie = b.Cookies[antiName];
        var overrideCookies = new Dictionary<string, string> { [antiName] = foreignAntiCookie };
        var before = await Rows(factory);
        foreach (var route in new[] { "/api/expenses", "/api/register", "/api/login", "/api/logout" })
        {
            using var rejected = await a.Send(HttpMethod.Post, route,
                new { amount = 11m, note = "foreign CSRF", email = "new-csrf@example.test", password = "Fictional-csrf-123!" },
                tokenB, overrideCookies);
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
            Assert.Equal("{\"error\":\"csrf\"}", await rejected.Content.ReadAsStringAsync());
            Assert.Equal(before, await Rows(factory));
        }
        using (var scope = factory.Services.CreateScope())
            Assert.Equal(2, await scope.ServiceProvider.GetRequiredService<AuthDb>().Users.CountAsync());
        using var created = await a.Send(HttpMethod.Post, "/api/expenses", new { amount = 11m, note = "session persists" }, tokenA);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<RowResponse>())!;
        using var wrongPut = await a.Send(HttpMethod.Put, $"/api/expenses/{item.Id}", new { amount = 22m, note = "foreign update" }, tokenB, overrideCookies);
        Assert.Equal(HttpStatusCode.BadRequest, wrongPut.StatusCode);
        Assert.Equal(new[] { new StoredRow(item.Id, ownerA, 11m, "session persists") }, await Rows(factory));
        using var missingLogout = await a.Send(HttpMethod.Post, "/api/logout");
        Assert.Equal(HttpStatusCode.BadRequest, missingLogout.StatusCode);
        using var stillAuthenticated = await a.Send(HttpMethod.Get, $"/api/expenses/{item.Id}");
        Assert.Equal(HttpStatusCode.OK, stillAuthenticated.StatusCode);
        using var logout = await a.Send(HttpMethod.Post, "/api/logout", token: tokenA);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        using var denied = await a.Send(HttpMethod.Get, $"/api/expenses/{item.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        Assert.Equal(new[] { new StoredRow(item.Id, ownerA, 11m, "session persists") }, await Rows(factory));
    });

    private static async Task WithFixture(Func<WebApplicationFactory<Program>, TestBrowser, TestBrowser, string, string, Task> body)
    {
        var previous = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", $"RoadmapAuth11_{Guid.NewGuid():N}");
        try
        {
            await using var factory = new WebApplicationFactory<Program>();
            using var a = new TestBrowser(factory);
            using var b = new TestBrowser(factory);
            var ownerA = await Register(a, "storage-a@example.test");
            var ownerB = await Register(b, "storage-b@example.test");
            await body(factory, a, b, ownerA, ownerB);
        }
        finally { Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", previous); }
    }

    private static async Task<string> Register(TestBrowser browser, string email)
    {
        using var response = await browser.Send(HttpMethod.Post, "/api/register",
            new { email, password = $"Fictional-{Guid.NewGuid():N}!" }, await browser.Token());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Registered>())!.UserId;
    }

    private static async Task<StoredRow[]> Rows(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AuthDb>().Expenses.AsNoTracking()
            .OrderBy(x => x.Id).Select(x => new StoredRow(x.Id, x.OwnerUserId, x.Amount, x.Note)).ToArrayAsync();
    }

    // Manual jars combine actual framework cookies to isolate CSRF identity binding.
    // They simulate TestServer requests and are not browser/TLS evidence.
    private sealed class TestBrowser : IDisposable
    {
        private readonly HttpClient client;
        public Dictionary<string, string> Cookies { get; } = new(StringComparer.Ordinal);
        public TestBrowser(WebApplicationFactory<Program> factory)
        {
            client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, AllowAutoRedirect = false });
            client.BaseAddress = new Uri("https://localhost");
        }
        public async Task<string> Token()
        {
            using var response = await Send(HttpMethod.Get, "/api/csrf");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<TokenResponse>())!.Token;
        }
        public async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body = null,
            string? token = null, Dictionary<string, string>? overrides = null, bool hasBody = false)
        {
            using var request = new HttpRequestMessage(method, path);
            if (body is not null || hasBody) request.Content = JsonContent.Create(body);
            var jar = new Dictionary<string, string>(Cookies);
            if (overrides is not null) foreach (var pair in overrides) jar[pair.Key] = pair.Value;
            if (jar.Count > 0) request.Headers.Add("Cookie", string.Join("; ", jar.Select(x => $"{x.Key}={x.Value}")));
            if (token is not null) request.Headers.Add("X-CSRF-TOKEN", token);
            if (method == HttpMethod.Post && path == "/api/expenses") request.Headers.Add("Idempotency-Key", $"c2-{Guid.NewGuid():N}");
            var response = await client.SendAsync(request);
            if (response.Headers.TryGetValues("Set-Cookie", out var values))
                foreach (var value in values)
                {
                    var pair = value.Split(';', 2)[0].Split('=', 2);
                    if (pair.Length != 2) continue;
                    if (pair[1].Length == 0) Cookies.Remove(pair[0]); else Cookies[pair[0]] = pair[1];
                }
            return response;
        }
        public void Dispose() => client.Dispose();
    }
    private sealed record StoredRow(int Id, string Owner, decimal Amount, string Note);
    private sealed record RowResponse(int Id, string Amount, string Note);
    private sealed record Registered(string UserId);
    private sealed record TokenResponse(string Header, string Token);
}
