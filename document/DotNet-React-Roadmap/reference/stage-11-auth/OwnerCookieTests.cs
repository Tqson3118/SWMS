using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class OwnerCookieTests
{
    [Fact]
    public async Task Valid_framework_protected_owner_ticket_reaches_owner_boundary()
    {
        await WithFixture(async (factory, client, userId, password) =>
        {
            AttachTicket(client, factory, userId, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/expenses/999999")).StatusCode);
        });
    }

    [Fact]
    public async Task Missing_name_identifier_claim_is_denied()
    {
        await WithFixture(async (factory, client, userId, password) =>
        {
            AttachTicket(client, factory, null, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/expenses/999999")).StatusCode);
        });
    }

    [Fact]
    public async Task Blank_name_identifier_claim_is_denied()
    {
        await WithFixture(async (factory, client, userId, password) =>
        {
            AttachTicket(client, factory, "", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/expenses/999999")).StatusCode);
        });
    }

    [Fact]
    public async Task Whitespace_name_identifier_claim_is_denied()
    {
        await WithFixture(async (factory, client, userId, password) =>
        {
            AttachTicket(client, factory, "   ", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/expenses/999999")).StatusCode);
        });
    }

    [Fact]
    public async Task Expired_valid_owner_ticket_is_rejected()
    {
        await WithFixture(async (factory, client, userId, password) =>
        {
            AttachTicket(client, factory, userId, DateTimeOffset.UtcNow.AddMinutes(-10), DateTimeOffset.UtcNow.AddMinutes(-1));
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/expenses/999999")).StatusCode);
        });
    }

    [Fact]
    public async Task Invalid_credentials_fail_for_existing_account_and_valid_login_succeeds()
    {
        await WithFixture(async (factory, client, userId, password) =>
        {
            var csrf = await GetCsrf(client);
            using var bad = new HttpRequestMessage(HttpMethod.Post, "/api/login")
            {
                Content = JsonContent.Create(new { email = "owner-cookie@example.test", password = "Wrong-password-123!" })
            };
            bad.Headers.Add("X-CSRF-TOKEN", csrf);
            var beforeBadLogin = await ReadAccountSnapshot(factory, userId);
            var badResponse = await client.SendAsync(bad);
            Assert.Equal(HttpStatusCode.Unauthorized, badResponse.StatusCode);
            var cookieOptions = factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(IdentityConstants.ApplicationScheme);
            Assert.False(badResponse.Headers.TryGetValues("Set-Cookie", out var setCookies) && setCookies.Any(x => x.Contains(cookieOptions.Cookie.Name!, StringComparison.Ordinal)));
            var badBody = await badResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Wrong-password-123!", badBody, StringComparison.Ordinal);
            Assert.DoesNotContain("owner-cookie@example.test", badBody, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/expenses/999999")).StatusCode);
            var afterBadLogin = await ReadAccountSnapshot(factory, userId);
            Assert.Equal(beforeBadLogin, afterBadLogin);

            var loginCsrf = await GetCsrf(client);
            using var good = new HttpRequestMessage(HttpMethod.Post, "/api/login")
            {
                Content = JsonContent.Create(new { email = "owner-cookie@example.test", password })
            };
            good.Headers.Add("X-CSRF-TOKEN", loginCsrf);
            Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(good)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/expenses/999999")).StatusCode);
        });
    }

    private static async Task WithFixture(Func<WebApplicationFactory<Program>, HttpClient, string, string, Task> body)
    {
        var previous = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
        var dbName = $"RoadmapAuth11_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", dbName);
        try
        {
            await using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            client.BaseAddress = new Uri("https://localhost");
            var password = $"Owner-cookie-{Guid.NewGuid():N}!";
            var csrf = await GetCsrf(client);
            using var register = new HttpRequestMessage(HttpMethod.Post, "/api/register")
            {
                Content = JsonContent.Create(new { email = "owner-cookie@example.test", password })
            };
            register.Headers.Add("X-CSRF-TOKEN", csrf);
            var response = await client.SendAsync(register);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            Assert.NotNull(created);
            Assert.False(string.IsNullOrWhiteSpace(created!.UserId));
            using var freshClient = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            freshClient.BaseAddress = new Uri("https://localhost");
            await body(factory, freshClient, created.UserId, password);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ROADMAP_AUTH_DB", previous);
        }
    }

    private static void AttachTicket(HttpClient client, WebApplicationFactory<Program> factory, string? ownerId, DateTimeOffset issued, DateTimeOffset expires)
    {
        var options = factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(IdentityConstants.ApplicationScheme);
        var claims = new List<Claim> { new(ClaimTypes.Name, "owner-cookie@example.test") };
        if (ownerId is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, ownerId));
        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var properties = new AuthenticationProperties { IssuedUtc = issued, ExpiresUtc = expires, IsPersistent = false };
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, IdentityConstants.ApplicationScheme);
        var protectedValue = options.TicketDataFormat!.Protect(ticket);
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", $"{options.Cookie.Name}={protectedValue}");
    }

    private static async Task<AccountSnapshot> ReadAccountSnapshot(WebApplicationFactory<Program> factory, string userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
        var user = await db.Users.AsNoTracking().SingleAsync(x => x.Id == userId);
        return new AccountSnapshot(user.Id, user.SecurityStamp, user.AccessFailedCount, await db.Expenses.AsNoTracking().CountAsync(x => x.OwnerUserId == userId));
    }

    private sealed record AccountSnapshot(string Id, string? SecurityStamp, int AccessFailedCount, int ExpenseCount);

    private static async Task<string> GetCsrf(HttpClient client) => (await client.GetFromJsonAsync<CsrfResponse>("/api/csrf"))!.Token;
    private sealed record CsrfResponse(string Header, string Token);
    private sealed record RegisterResponse(string UserId);
}



