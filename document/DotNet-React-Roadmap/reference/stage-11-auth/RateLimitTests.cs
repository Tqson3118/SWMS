using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public sealed class RateLimitTests : IAsyncLifetime
{
    private RateFactory _factory = null!;
    private string _db = "";
    private string? _previousDb;

    public Task InitializeAsync()
    {
        _previousDb = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        _db = $"RoadmapAuth11_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", _db);
        try
        {
            _factory = new RateFactory();
        }
        catch
        {
            Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", _previousDb);
            throw;
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_third_same_direct_ip_is_exact_429_without_queue()
    {
        using var c = Client("10.11.0.1");
        Assert.Equal(HttpStatusCode.OK, (await Register(c, "r1@example.test")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Register(c, "r2@example.test")).StatusCode);
        var rejected = await Register(c, "r3@example.test");
        await Assert429(rejected);
        await using var scope = _factory.Services.CreateAsyncScope();
        Assert.Equal(2, await scope.ServiceProvider.GetRequiredService<AuthDb>().Users.CountAsync());
    }

    [Fact]
    public async Task Anonymous_buckets_are_separate_and_forwarded_header_does_not_bypass()
    {
        using var a = Client("10.11.0.2");
        Assert.Equal(HttpStatusCode.OK, (await Register(a, "a1@example.test")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Register(a, "a2@example.test")).StatusCode);
        using var b = Client("10.11.0.3");
        Assert.Equal(HttpStatusCode.OK, (await Register(b, "b1@example.test")).StatusCode);
        a.DefaultRequestHeaders.Add("X-Forwarded-For", "10.11.0.3");
        await Assert429(await Register(a, "a3@example.test"));
    }

    [Fact]
    public async Task Authenticated_owner_buckets_are_separate_and_reads_remain_available()
    {
        using var a = Client("10.11.0.4");
        using var b = Client("10.11.0.5");
        Assert.Equal(HttpStatusCode.OK, (await Register(a, "owner-a@example.test")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Register(b, "owner-b@example.test")).StatusCode);
        SetIp(a, "10.11.0.9"); SetIp(b, "10.11.0.9");
        var ta = await Csrf(a); var tb = await Csrf(b);
        Assert.Equal(HttpStatusCode.Created, (await Expense(a, ta, 1m, "a1")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await Expense(a, ta, 2m, "a2")).StatusCode);
        await Assert429(await Expense(a, ta, 3m, "a3"));
        Assert.Equal(HttpStatusCode.Created, (await Expense(b, tb, 4m, "b1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await b.GetAsync("/api/csrf")).StatusCode);
    }

    [Fact]
    public async Task Existing_auth_and_csrf_precedence_remain_before_limiter()
    {
        using var anonymous = Client("10.11.0.6");
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/api/expenses", new { amount = 1m, note = "x" })).StatusCode);
        using var c = Client("10.11.0.7");
        Assert.Equal(HttpStatusCode.OK, (await Register(c, "csrf@example.test")).StatusCode);
        var bad = await c.PostAsJsonAsync("/api/expenses", new { amount = 1m, note = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    [Fact]
    public async Task Login_third_request_is_429_without_set_cookie_or_user_mutation()
    {
        using var setup = Client("10.11.0.8");
        Assert.Equal(HttpStatusCode.OK, (await Register(setup, "login@example.test")).StatusCode);
        SetIp(setup, "10.11.0.18");
        var before = await UserCount();
        Assert.Equal(HttpStatusCode.OK, (await Login(setup, "login@example.test")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Login(setup, "login@example.test")).StatusCode);
        var rejected = await Login(setup, "login@example.test");
        await Assert429(rejected);
        Assert.Empty(rejected.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : Array.Empty<string>());
        Assert.Equal(before, await UserCount());
    }

    [Fact]
    public async Task Logout_third_request_is_429_and_replayed_server_cookie_stays_usable()
    {
        using var raw = RawClient("10.11.0.19");
        var csrfResponse = await raw.GetAsync("/api/csrf");
        var csrf = (await csrfResponse.Content.ReadFromJsonAsync<CsrfResponse>())!.Token;
        var antiCookie = CookiePair(csrfResponse, "RoadmapAuth11.XSRF");
        raw.DefaultRequestHeaders.Add("Cookie", antiCookie);
        using var register = new HttpRequestMessage(HttpMethod.Post, "/api/register") { Content = JsonContent.Create(new { email = "logout@example.test", password = "A-valid-password-123!" }) };
        register.Headers.Add("X-CSRF-TOKEN", csrf);
        var registered = await raw.SendAsync(register);
        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);
        var authCookie = CookiePair(registered, ".AspNetCore.Identity.Application");
        using var replay = ManualClient("10.11.0.20", $"{authCookie}; {antiCookie}");
        var currentAntiCookie = registered.Headers.TryGetValues("Set-Cookie", out _) && registered.Headers.TryGetValues("Set-Cookie", out var registrationCookies) && registrationCookies.Any(x => x.StartsWith("RoadmapAuth11.XSRF=", StringComparison.Ordinal)) ? CookiePair(registered, "RoadmapAuth11.XSRF") : antiCookie;
        replay.DefaultRequestHeaders.Remove("Cookie"); replay.DefaultRequestHeaders.Add("Cookie", $"{authCookie}; {currentAntiCookie}");
        var replayCsrfResponse = await replay.GetAsync("/api/csrf");
        var replayToken = (await replayCsrfResponse.Content.ReadFromJsonAsync<CsrfResponse>())!.Token;
        replay.DefaultRequestHeaders.Remove("Cookie"); replay.DefaultRequestHeaders.Add("Cookie", $"{authCookie}; {currentAntiCookie}");
        Assert.Equal(HttpStatusCode.NoContent, (await Logout(replay, replayToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Logout(replay, replayToken)).StatusCode);
        var rejected = await Logout(replay, replayToken);
        await Assert429(rejected);
        Assert.Empty(rejected.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : Array.Empty<string>());
        Assert.Equal(HttpStatusCode.OK, (await replay.GetAsync("/api/csrf")).StatusCode);
    }

    [Fact]
    public async Task Put_exhaustion_is_429_without_mutation_and_protected_get_still_works()
    {
        using var c = Client("10.11.0.21");
        Assert.Equal(HttpStatusCode.OK, (await Register(c, "put@example.test")).StatusCode);
        var token = await Csrf(c);
        var created = await Expense(c, token, 11m, "original");
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        SetIp(c, "10.11.0.211");
        Assert.Equal(HttpStatusCode.OK, (await PutExpense(c, token, id, 12m, "changed-1")).StatusCode);
        var before = await ExpenseSnapshot(id);
        var rejected = await PutExpense(c, token, id, 99m, "must-not-write");
        await Assert429(rejected);
        Assert.Empty(rejected.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : Array.Empty<string>());
        Assert.Equal(before, await ExpenseSnapshot(id));
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync($"/api/expenses/{id}")).StatusCode);
    }

    [Fact]
    public async Task Null_remote_ip_uses_bounded_fallback_while_owner_partitions_and_gets_work()
    {
        using var a = Client("10.11.0.22");
        using var b = Client("10.11.0.23");
        Assert.Equal(HttpStatusCode.OK, (await Register(a, "null-a@example.test")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Register(b, "null-b@example.test")).StatusCode);
        SetNullIp(a); SetNullIp(b);
        var ta = await Csrf(a); var tb = await Csrf(b);
        Assert.Equal(HttpStatusCode.Created, (await Expense(a, ta, 21m, "a")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await Expense(a, ta, 22m, "a2")).StatusCode);
        await Assert429(await Expense(a, ta, 23m, "a3"));
        var bCreated = await Expense(b, tb, 24m, "b");
        Assert.Equal(HttpStatusCode.Created, bCreated.StatusCode);
        var bId = (await bCreated.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await b.GetAsync($"/api/expenses/{bId}")).StatusCode);
    }

    private HttpClient Client(string ip)
    {
        var c = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
        c.BaseAddress = new Uri("https://localhost"); SetIp(c, ip); return c;
    }
    private static void SetIp(HttpClient c, string ip) { c.DefaultRequestHeaders.Remove("X-Test-Remote-IP"); c.DefaultRequestHeaders.Add("X-Test-Remote-IP", ip); }
    private static async Task<HttpResponseMessage> Register(HttpClient c, string email)
    {
        var token = await Csrf(c); using var r = new HttpRequestMessage(HttpMethod.Post, "/api/register") { Content = JsonContent.Create(new { email, password = "A-valid-password-123!" }) }; r.Headers.Add("X-CSRF-TOKEN", token); return await c.SendAsync(r);
    }
    private static async Task<string> Csrf(HttpClient c) => (await c.GetFromJsonAsync<CsrfResponse>("/api/csrf"))!.Token;
    private static async Task<HttpResponseMessage> Expense(HttpClient c, string token, decimal amount, string note)
    { using var r = new HttpRequestMessage(HttpMethod.Post, "/api/expenses") { Content = JsonContent.Create(new { amount, note }) }; r.Headers.Add("X-CSRF-TOKEN", token); r.Headers.Add("Idempotency-Key", $"c2-{Guid.NewGuid():N}"); return await c.SendAsync(r); }
    private static async Task<HttpResponseMessage> Login(HttpClient c, string email)
    { var token = await Csrf(c); using var r = new HttpRequestMessage(HttpMethod.Post, "/api/login") { Content = JsonContent.Create(new { email, password = "A-valid-password-123!" }) }; r.Headers.Add("X-CSRF-TOKEN", token); return await c.SendAsync(r); }
    private static async Task<HttpResponseMessage> Logout(HttpClient c, string token)
    { using var r = new HttpRequestMessage(HttpMethod.Post, "/api/logout"); r.Headers.Add("X-CSRF-TOKEN", token); return await c.SendAsync(r); }
    private static async Task<HttpResponseMessage> PutExpense(HttpClient c, string token, int id, decimal amount, string note)
    { using var r = new HttpRequestMessage(HttpMethod.Put, $"/api/expenses/{id}") { Content = JsonContent.Create(new { amount, note, ownerUserId = "client-value-ignored" }) }; r.Headers.Add("X-CSRF-TOKEN", token); return await c.SendAsync(r); }
    private async Task<int> UserCount()
    { await using var scope = _factory.Services.CreateAsyncScope(); return await scope.ServiceProvider.GetRequiredService<AuthDb>().Users.CountAsync(); }
    private async Task<string> ExpenseSnapshot(int id)
    { await using var scope = _factory.Services.CreateAsyncScope(); var e = await scope.ServiceProvider.GetRequiredService<AuthDb>().Expenses.SingleAsync(x => x.Id == id); return $"{e.Amount:0.00}|{e.Note}|{e.OwnerUserId}"; }
    private HttpClient ManualClient(string ip, string cookie)
    { var c = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, AllowAutoRedirect = false }); c.BaseAddress = new Uri("https://localhost"); SetIp(c, ip); c.DefaultRequestHeaders.Add("Cookie", cookie); return c; }
    private HttpClient RawClient(string ip)
    { var c = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, AllowAutoRedirect = false }); c.BaseAddress = new Uri("https://localhost"); SetIp(c, ip); return c; }
    private static string CookiePair(HttpResponseMessage response, string name)
    { var raw = response.Headers.GetValues("Set-Cookie").Single(x => x.StartsWith(name + "=", StringComparison.Ordinal)); return raw[..raw.IndexOf(';')]; }
    private static void SetNullIp(HttpClient c) { c.DefaultRequestHeaders.Remove("X-Test-Remote-IP"); c.DefaultRequestHeaders.Add("X-Test-Remote-IP", "null"); }
    private static async Task Assert429(HttpResponseMessage response)
    { Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode); Assert.Equal("60", response.Headers.GetValues("Retry-After").Single()); var body = await response.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal("rate limit exceeded", body.GetProperty("error").GetString()); Assert.Single(body.EnumerateObject()); }
    public Task DisposeAsync()
    {
        _factory?.Dispose();
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", _previousDb);
        return Task.CompletedTask;
    }
    private sealed record CsrfResponse(string Header, string Token);
}

public sealed class RateFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<RateLimitSettings>();
            services.AddSingleton(new RateLimitSettings(2, 60));
            services.AddSingleton<IStartupFilter>(new TestRemoteIpStartupFilter());
        });
    }
}

public sealed class TestRemoteIpStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (ctx, continuation) =>
        {
            var requested = ctx.Request.Headers["X-Test-Remote-IP"].FirstOrDefault();
            if (string.Equals(requested, "null", StringComparison.Ordinal)) ctx.Connection.RemoteIpAddress = null;
            else if (IPAddress.TryParse(requested, out var address)) ctx.Connection.RemoteIpAddress = address;
            await continuation();
        });
        next(app);
    };
}
