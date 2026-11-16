using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
AuthStartupGuard.Validate(builder.Configuration);
builder.WebHost.UseUrls("http://127.0.0.1:0");
var dbName = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB");
if (string.IsNullOrWhiteSpace(dbName)) throw new InvalidOperationException("ROADMAP_AUTH_DB is required");
if (!AuthStartupGuard.IsValidDatabaseName(dbName))
    throw new InvalidOperationException("Strict auth DB name required");
var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Integrated Security=True;TrustServerCertificate=True;";
Console.Error.WriteLine($"AUTH_DB_BEFORE_CREATE name={dbName} server=(localdb)\\MSSQLLocalDB integratedSecurity=true");
await RefuseExistingDatabaseAsync(dbName);

builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<OrderManagement.IdempotencyCommandObserver>();
builder.Services.AddDbContext<AuthDb>((services, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(services.GetRequiredService<OrderManagement.IdempotencyCommandObserver>());
});
builder.Services.AddScoped<OrderManagement.ExpenseIdempotencyService>();
builder.Services.AddIdentity<LearningUser, IdentityRole>(o =>
{
    o.Password.RequiredLength = 12;
    o.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AuthDb>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = 401; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.Name = "RoadmapAuth11.XSRF";
    o.Cookie.HttpOnly = false;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton(new RateLimitSettings(64, 60));
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "rate limit exceeded" }, cancellationToken);
    };
    options.AddPolicy("anonymous-auth", context =>
    {
        var settings = context.RequestServices.GetRequiredService<RateLimitSettings>();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter($"anonymous:{ip}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
    options.AddPolicy("authenticated-write", context =>
    {
        var settings = context.RequestServices.GetRequiredService<RateLimitSettings>();
        var owner = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var key = string.IsNullOrWhiteSpace(owner) ? "missing" : owner;
        return RateLimitPartition.GetFixedWindowLimiter($"owner:{key}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

var app = builder.Build();
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // This educational fixture keeps a code/trace diagnostic rather than raw exceptions.
    SuppressDiagnosticsCallback = _ => true,
    ExceptionHandler = async context =>
    {
        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RoadmapAuth11.SafeErrors")
            .LogError("auth_server_error trace={TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "server error", traceId = context.TraceIdentifier });
    }
});
await using (var scope = app.Services.CreateAsyncScope())
await scope.ServiceProvider.GetRequiredService<AuthDb>().Database.MigrateAsync();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Method is "POST" or "PUT" or "DELETE" && ctx.Request.Path.StartsWithSegments("/api") && !ctx.Request.Path.StartsWithSegments("/api/csrf"))
    {
        var antiforgery = ctx.RequestServices.GetRequiredService<IAntiforgery>();
        if (!ctx.Request.Headers.ContainsKey("X-CSRF-TOKEN"))
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsJsonAsync(new { error = "csrf" });
            return;
        }
        try
        {
            await antiforgery.ValidateRequestAsync(ctx);
        }
        catch (AntiforgeryValidationException)
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsJsonAsync(new { error = "csrf" });
            return;
        }
    }
    await next();
});
app.UseAntiforgery();
app.UseRateLimiter();

app.MapGet("/api/csrf", (HttpContext http, IAntiforgery anti) =>
{
    var tokens = anti.GetAndStoreTokens(http);
    return Results.Ok(new { header = "X-CSRF-TOKEN", token = tokens.RequestToken });
});
app.MapPost("/api/register", async (RegisterRequest? request, UserManager<LearningUser> users, SignInManager<LearningUser> signIn) =>
{
    if (request is null || !new EmailAddressAttribute().IsValid(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { error = "invalid registration" });
    var user = new LearningUser { UserName = request.Email, Email = request.Email };
    var result = await users.CreateAsync(user, request.Password);
    if (!result.Succeeded) return Results.BadRequest(new { error = "registration rejected" });
    await signIn.SignInAsync(user, false);
    return Results.Ok(new { userId = user.Id });
}).RequireRateLimiting("anonymous-auth");
app.MapPost("/api/login", async (LoginRequest? request, SignInManager<LearningUser> signIn) =>
{
    if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return Results.BadRequest(new { error = "invalid login" });
    var result = await signIn.PasswordSignInAsync(request.Email, request.Password, false, false);
    return result.Succeeded ? Results.Ok() : Results.Unauthorized();
}).RequireRateLimiting("anonymous-auth");
app.MapPost("/api/logout", async (ClaimsPrincipal principal, SignInManager<LearningUser> signIn) =>
{
    if (!(principal.Identity?.IsAuthenticated ?? false)) return Results.Unauthorized();
    await signIn.SignOutAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("authenticated-write");

var expenses = app.MapGroup("/api/expenses").RequireAuthorization();
expenses.MapPost("", async (ExpenseRequest? request, ClaimsPrincipal principal, HttpRequest httpRequest, OrderManagement.ExpenseIdempotencyService idempotency, CancellationToken cancellationToken) =>
{
    var owner = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(owner)) return Results.Unauthorized();
    if (!TryValidateExpense(request, out var amount, out var note, out var error)) return Results.BadRequest(new { error });
    try
    {
        var result = await idempotency.CreateOrReplayAsync(owner, httpRequest.Headers["Idempotency-Key"].ToArray(), amount, note, cancellationToken);
        var location = $"/api/expenses/{result.ExpenseId}";
        return Results.Created(location, new { id = result.ExpenseId, amount = result.Amount.ToString("0.00", CultureInfo.InvariantCulture), note = result.Note });
    }
    catch (ArgumentOutOfRangeException) { return Results.BadRequest(new { error = "invalid expense" }); }
    catch (ArgumentException) { return Results.BadRequest(new { error = "invalid idempotency key" }); }
    catch (OrderManagement.IdempotencyConflictException) { return Results.Conflict(new { error = "idempotency conflict" }); }
}).RequireRateLimiting("authenticated-write");
expenses.MapGet("/{id:int}", async (int id, ClaimsPrincipal principal, AuthDb db, CancellationToken cancellationToken) =>
{
    var owner = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(owner)) return Results.Unauthorized();
    var expense = await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OwnerUserId == owner, cancellationToken);
    return expense is null ? Results.NotFound() : Results.Ok(new { expense.Id, amount = expense.Amount.ToString("0.00", CultureInfo.InvariantCulture), expense.Note });
});
expenses.MapPut("/{id:int}", async (int id, ExpenseRequest? request, ClaimsPrincipal principal, AuthDb db, CancellationToken cancellationToken) =>
{
    var owner = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(owner)) return Results.Unauthorized();
    var expense = await db.Expenses.SingleOrDefaultAsync(x => x.Id == id && x.OwnerUserId == owner, cancellationToken);
    if (expense is null) return Results.NotFound();
    if (!TryValidateExpense(request, out var amount, out var note, out var error)) return Results.BadRequest(new { error });
    expense.Amount = amount;
    expense.Note = note;
    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(new { expense.Id, amount = expense.Amount.ToString("0.00", CultureInfo.InvariantCulture), expense.Note });
}).RequireRateLimiting("authenticated-write");
app.Run();

static bool TryValidateExpense(ExpenseRequest? request, out decimal amount, out string note, out string error)
{
    amount = 0; note = ""; error = "invalid expense";
    if (request?.Amount is not decimal supplied || supplied <= 0 || supplied > 1_000_000 || decimal.Round(supplied, 2) != supplied) { error = "invalid amount"; return false; }
    if (request.Note is null || request.Note.Length > 500) { error = "invalid note"; return false; }
    amount = supplied; note = request.Note; return true;
}

static async Task RefuseExistingDatabaseAsync(string name)
{
    await using var connection = new Microsoft.Data.SqlClient.SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;");
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name=@name";
    command.Parameters.AddWithValue("@name", name);
    if ((int)(await command.ExecuteScalarAsync())! > 0) throw new InvalidOperationException("Refusing existing auth database");
}

public static class AuthStartupGuard
{
    private const string AllowedUrl = "http://127.0.0.1:0";
    public static bool IsValidDatabaseName(string? name) => name is not null && System.Text.RegularExpressions.Regex.IsMatch(name, @"\ARoadmapAuth11_[0-9a-f]{32}\z");
    public static void Validate(IConfiguration configuration)
    {
        // Inspect both sources: an allowed environment value must not hide a bad override.
        var values = new[] { "urls", "URLS", "ASPNETCORE_URLS" }
            .SelectMany(key => new[] { configuration[key], Environment.GetEnvironmentVariable(key) })
            .Where(value => value is not null).Cast<string>().ToArray();
        foreach (var value in values)
            if (value.Length == 0 || value != AllowedUrl) throw new InvalidOperationException("Only the exact ephemeral loopback URL is allowed");
        if (configuration.GetSection("Kestrel:Endpoints").GetChildren().Any()) throw new InvalidOperationException("Named Kestrel endpoints are not allowed in this fixture");
        foreach (var key in Environment.GetEnvironmentVariables().Keys.Cast<object>().Select(key => key.ToString()!).Where(key => key.StartsWith("Kestrel__Endpoints__", StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Named Kestrel endpoint environment configuration is not allowed");
    }
}
public sealed record RegisterRequest([Required, EmailAddress] string Email, [Required, MinLength(12)] string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record ExpenseRequest(decimal? Amount, string? Note, string? OwnerUserId);
public sealed class LearningUser : IdentityUser { }
public sealed class Expense { public int Id { get; set; } public string OwnerUserId { get; set; } = ""; public decimal Amount { get; set; } public string Note { get; set; } = ""; }
public sealed partial class AuthDb(DbContextOptions<AuthDb> options) : IdentityDbContext<LearningUser>(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Expense>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Expense>().HasIndex(x => new { x.OwnerUserId, x.Id }).IsUnique();
        ConfigureReceiptModel(modelBuilder);
    }

    partial void ConfigureReceiptModel(ModelBuilder modelBuilder);
}
public partial class Program { }

public sealed record RateLimitSettings
{
    public int PermitLimit { get; }
    public int WindowSeconds { get; }
    public RateLimitSettings(int permitLimit, int windowSeconds)
    {
        if (permitLimit is < 1 or > 64) throw new ArgumentOutOfRangeException(nameof(permitLimit));
        if (windowSeconds != 60) throw new ArgumentOutOfRangeException(nameof(windowSeconds));
        PermitLimit = permitLimit;
        WindowSeconds = windowSeconds;
    }
}



