using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swms.Api.Features;
using Swms.Api.Features.Pdf;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Ensure default listening URL is port 5080 (aligned with Next.js BFF rewrite destination)
if (string.IsNullOrEmpty(builder.Configuration["Urls"]) &&
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://127.0.0.1:5080");
}

// 1. Dual-Mode Database Persistence:
// Uses PostgreSQL 17 if reachable (or forced via SWMS_USE_POSTGRES=true),
// otherwise gracefully falls back to InMemory database with Vietnamese Seed Data so local dev & E2E run immediately.
var usePostgresEnv = Environment.GetEnvironmentVariable("SWMS_USE_POSTGRES");
var postgresConn = builder.Configuration.GetConnectionString("Postgres");
bool isPostgresAvailable = false;

if (string.Equals(usePostgresEnv, "true", StringComparison.OrdinalIgnoreCase))
{
    isPostgresAvailable = true;
}
else if (!string.Equals(usePostgresEnv, "false", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        using var tcp = new TcpClient();
        var connectTask = tcp.ConnectAsync("127.0.0.1", 5432);
        if (connectTask.Wait(TimeSpan.FromMilliseconds(350)) && tcp.Connected)
        {
            isPostgresAvailable = true;
        }
    }
    catch
    {
        isPostgresAvailable = false;
    }
}

bool isRunningUnderTests = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name?.Contains("testhost", StringComparison.OrdinalIgnoreCase) == true)
                            || !string.IsNullOrWhiteSpace(builder.Configuration["Database:InMemoryName"]);

var rawSqliteConn = builder.Configuration.GetConnectionString("SqliteFallback");
string sqliteConn;
if (string.IsNullOrWhiteSpace(rawSqliteConn))
{
    var rootDir = Directory.Exists(@"D:\03_Study\Studxy") ? @"D:\03_Study\Studxy" : AppContext.BaseDirectory;
    sqliteConn = $"Data Source={Path.Combine(rootDir, "swms_local_dev.db")}";
}
else if (rawSqliteConn.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) && !Path.IsPathRooted(rawSqliteConn[12..].Trim()))
{
    var rel = rawSqliteConn[12..].Trim();
    var rootDir = Directory.Exists(@"D:\03_Study\Studxy") ? @"D:\03_Study\Studxy" : AppContext.BaseDirectory;
    sqliteConn = $"Data Source={Path.Combine(rootDir, rel)}";
}
else
{
    sqliteConn = rawSqliteConn;
}

builder.Services.AddDbContext<SwmsDbContext>(options =>
{
    if (isPostgresAvailable && !string.IsNullOrWhiteSpace(postgresConn))
    {
        options.UseNpgsql(postgresConn);
    }
    else if (isRunningUnderTests)
    {
        var inMem = builder.Configuration["Database:InMemoryName"] ?? "SwmsAuthoritativeDevDb";
        options.UseInMemoryDatabase(inMem);
    }
    else
    {
        // Persistent SQLite for local app so courses and learning progress survive restarts
        options.UseSqlite(sqliteConn);
    }
});

// 2. Security & Auth Services (JWT + AES-256-GCM RTR + Token Version Verification)
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new AesGcmTokenProtector(cfg["Security:AesMasterKeyBase64"]);
});
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<RefreshTokenRotationService>();

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? "SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://api.studxy.vn";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://app.studxy.vn";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(15)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var sub = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? ctx.Principal?.FindFirst("sub")?.Value;
                var verClaim = ctx.Principal?.FindFirst("ver")?.Value;

                if (!Guid.TryParse(sub, out var userId) || !int.TryParse(verClaim, out var tokenVer))
                {
                    ctx.HttpContext.Items["JwtVerRevoked"] = "Invalid sub or ver claim in JWT.";
                    ctx.Fail("Invalid sub or ver claim in JWT.");
                    return;
                }

                var db = ctx.HttpContext.RequestServices.GetRequiredService<SwmsDbContext>();
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null || user.TokenVersion != tokenVer)
                {
                    ctx.HttpContext.Items["JwtVerRevoked"] = "TOKEN_VERSION_REVOKED";
                    ctx.Fail("JWT token_version has been revoked.");
                }
            },
            OnAuthenticationFailed = ctx =>
            {
                ctx.HttpContext.Items["JwtAuthFailed"] = ctx.Exception.Message;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 3. AI Proxy Client & Durable Background Queue (.NET 10 Bounded Channel<T> + SemaphoreSlim(5))
builder.Services.AddHttpClient<GeminiProxyClient>();
builder.Services.AddSingleton<AiTaskQueue>();
builder.Services.AddHostedService<AiJobProcessorService>();

// 3.1 PDF Extraction Engine (PdfPig pure C#)
builder.Services.AddSingleton<IPdfExtractionService, PdfExtractionService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 4. CORS for Next.js 15 Frontend (with credentials for HttpOnly RTR cookies)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwmsFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("SwmsFrontend");
app.UseAuthentication();
app.Use(async (ctx, next) =>
{
    // If client explicitly supplied an Authorization Bearer token and it failed validation or token_version check, reject with 401
    if (ctx.Request.Headers.TryGetValue("Authorization", out var authHeader) &&
        authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
        ctx.User.Identity?.IsAuthenticated != true)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await ctx.Response.WriteAsJsonAsync(new
        {
            error = ctx.Items["JwtVerRevoked"]?.ToString() ?? ctx.Items["JwtAuthFailed"]?.ToString() ?? "Unauthorized bearer token.",
            tokenVersionRevoked = ctx.Items.ContainsKey("JwtVerRevoked")
        });
        return;
    }
    await next();
});
app.UseAuthorization();

// Seed Vietnamese OS & Paging Data on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SwmsEndpoints.EnsureSqliteSchemaUpdatedAsync(db);
    await SeedDataInitializer.SeedAsync(db, builder.Configuration["DocumentLibrary:RootPath"]);
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "SWMS Backend .NET 10 LTS",
    databaseMode = isPostgresAvailable ? "PostgreSQL 17 + pgvector" : (isRunningUnderTests ? "Isolated In-Memory (Test Runner)" : "Persistent SQLite (swms_local_dev.db) + Vietnamese Seed Data"),
    aiEndpoint = builder.Configuration["AiProxy:BaseUrl"] ?? "http://127.0.0.1:8045",
    aiModel = builder.Configuration["AiProxy:Model"] ?? "gemini-3.8-flash"
}));

app.MapSwmsEndpoints();

app.Run();

public partial class Program { }
