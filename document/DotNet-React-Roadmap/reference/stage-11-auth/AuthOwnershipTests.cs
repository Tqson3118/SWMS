using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public sealed class AuthOwnershipTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _a = null!;
    private HttpClient _b = null!;
    private string _ap = "";
    private string _bp = "";
    private string? _previousDb;

    public async Task InitializeAsync()
    {
        _previousDb = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", $"RoadmapAuth11_{Guid.NewGuid():N}");
        _ap = $"A-only-{Guid.NewGuid():N}!";
        _bp = $"B-only-{Guid.NewGuid():N}!";
        try
        {
            _factory = new WebApplicationFactory<Program>();
            _a = NewClient();
            _b = NewClient();
            await Register(_a, "learner-a@example.test", _ap);
            await Register(_b, "learner-b@example.test", _bp);
        }
        catch
        {
            _a?.Dispose();
            _b?.Dispose();
            _factory?.Dispose();
            Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", _previousDb);
            throw;
        }
    }

    private HttpClient NewClient() { var c = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false }); c.BaseAddress = new Uri("https://localhost"); return c; }
    private static async Task<string> GetCsrf(HttpClient client) => (await client.GetFromJsonAsync<CsrfResponse>("/api/csrf"))!.Token;
    private static async Task Register(HttpClient client, string email, string password)
    {
        var token = await GetCsrf(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/register") { Content = JsonContent.Create(new { email, password }) };
        request.Headers.Add("X-CSRF-TOKEN", token);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(request)).StatusCode);
    }
    private static async Task<HttpResponseMessage> Create(HttpClient client, string token, decimal? amount, string note, string? owner = "attacker")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount, note, ownerUserId = owner }) };
        request.Headers.Add("Idempotency-Key", $"c2-{Guid.NewGuid():N}");
        if (token is not null) request.Headers.Add("X-CSRF-TOKEN", token);
        return await client.SendAsync(request);
    }

    [Fact] public async Task Anonymous_is_denied_for_read_and_write() { using var c = NewClient(); Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/expenses/1")).StatusCode); Assert.Equal(HttpStatusCode.Unauthorized, (await Create(c, null!, 12.34m, "anonymous")).StatusCode); }

    [Fact] public async Task Cookie_owner_filter_blocks_cross_owner_and_put_cannot_cross_owner()
    {
        var token = await GetCsrf(_a);
        var rejected = await Create(_a, null!, 12.34m, "A");
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        var created = await Create(_a, token, 12.34m, "A");
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var result = await created.Content.ReadFromJsonAsync<ExpenseResult>();
        Assert.NotNull(result);
        Assert.Equal("12.34", result!.Amount);
        Assert.Equal(HttpStatusCode.NotFound, (await _b.GetAsync($"/api/expenses/{result.Id}")).StatusCode);
        var bToken = await GetCsrf(_b);
        using var crossPut = new HttpRequestMessage(HttpMethod.Put, $"/api/expenses/{result.Id}") { Content = JsonContent.Create(new { amount = 99.99m, note = "B", ownerUserId = "attacker" }) };
        crossPut.Headers.Add("X-CSRF-TOKEN", bToken);
        Assert.Equal(HttpStatusCode.NotFound, (await _b.SendAsync(crossPut)).StatusCode);
        var unchanged = await _a.GetFromJsonAsync<ExpenseResult>($"/api/expenses/{result.Id}"); Assert.Equal("12.34", unchanged!.Amount);
        using var ownPut = new HttpRequestMessage(HttpMethod.Put, $"/api/expenses/{result.Id}") { Content = JsonContent.Create(new { amount = 13.25m, note = "A updated", ownerUserId = "attacker" }) };
        ownPut.Headers.Add("X-CSRF-TOKEN", token);
        var ownResponse = await _a.SendAsync(ownPut); Assert.Equal(HttpStatusCode.OK, ownResponse.StatusCode); var updated = await ownResponse.Content.ReadFromJsonAsync<ExpenseResult>(); Assert.Equal("13.25", updated!.Amount);
    }

    [Fact] public async Task Login_logout_and_duplicate_registration_use_csrf_and_real_cookie()
    {
        using var c = NewClient();
        var loginToken = await GetCsrf(c);
        using var login = new HttpRequestMessage(HttpMethod.Post, "/api/login") { Content = JsonContent.Create(new { email = "learner-a@example.test", password = _ap }) };
        login.Headers.Add("X-CSRF-TOKEN", loginToken);
        Assert.Equal(HttpStatusCode.OK, (await c.SendAsync(login)).StatusCode);
        var createToken = await GetCsrf(c);
        Assert.Equal(HttpStatusCode.Created, (await Create(c, createToken, 10.00m, "logged in", null)).StatusCode);
        var logoutToken = await GetCsrf(c);
        using var logout = new HttpRequestMessage(HttpMethod.Post, "/api/logout"); logout.Headers.Add("X-CSRF-TOKEN", logoutToken);
        Assert.Equal(HttpStatusCode.NoContent, (await c.SendAsync(logout)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/expenses/1")).StatusCode);
        var duplicate = await RegisterResponse(_a, "learner-a@example.test", _ap);
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
    }

    [Fact] public async Task Login_and_register_without_csrf_are_rejected()
    {
        using var c = NewClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/register", new { email = "new@example.test", password = "A-only-ephemeral-password!" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/login", new { email = "learner-a@example.test", password = _ap })).StatusCode);
    }

    [Fact] public void Endpoint_guard_rejects_every_non_exact_url_and_database_name()
    {
        foreach (var bad in new[] { " http://127.0.0.1:0", "http://127.0.0.1:0 ", "http://127.0.0.1:5000", "http://0.0.0.0:0", "http://127.0.0.1:0;http://127.0.0.1:1", "http://user:pass@127.0.0.1:0", "http://127.0.0.1:0/path", "http://127.0.0.1:0?x=y" })
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["urls"] = bad }).Build();
            Assert.Throws<InvalidOperationException>(() => AuthStartupGuard.Validate(config));
        }
        var named = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Kestrel:Endpoints:Http:Url"] = "http://127.0.0.1:0" }).Build();
        Assert.Throws<InvalidOperationException>(() => AuthStartupGuard.Validate(named));
        Assert.True(AuthStartupGuard.IsValidDatabaseName("RoadmapAuth11_0123456789abcdef0123456789abcdef"));
        Assert.False(AuthStartupGuard.IsValidDatabaseName("RoadmapAuth11_0123456789abcdef0123456789abcdef\n"));
        Assert.False(AuthStartupGuard.IsValidDatabaseName("RoadmapAuth11_0123456789abcdef0123456789abcde"));
    }
    [Fact] public async Task Invalid_cookie_and_invalid_amount_are_rejected_without_sensitive_error()
    {
        using var c = NewClient(); c.DefaultRequestHeaders.Add("Cookie", ".AspNetCore.Identity.Application=malformed");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/expenses/1")).StatusCode);
        var token = await GetCsrf(_a);
        Assert.Equal(HttpStatusCode.BadRequest, (await Create(_a, token, 12.345m, "fractional cents", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Create(_a, token, 1.00m, new string('x', 501), null)).StatusCode);
    }

    private static async Task<HttpResponseMessage> RegisterResponse(HttpClient client, string email, string password)
    {
        var token = await GetCsrf(client); using var request = new HttpRequestMessage(HttpMethod.Post, "/api/register") { Content = JsonContent.Create(new { email, password }) }; request.Headers.Add("X-CSRF-TOKEN", token); return await client.SendAsync(request);
    }
    public Task DisposeAsync()
    {
        _a?.Dispose();
        _b?.Dispose();
        _factory?.Dispose();
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", _previousDb);
        return Task.CompletedTask;
    }
    private sealed record CsrfResponse(string Header, string Token);
    private sealed record ExpenseResult(int Id, string Amount, string Note);
}





