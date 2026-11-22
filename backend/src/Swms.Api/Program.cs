using System.Data.Common;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Swms.Api.Features;
using Swms.Api.Features.Pdf;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.Security;

bool isTestHostProcess = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.GetName().Name?.Contains("testhost", StringComparison.OrdinalIgnoreCase) == true);

var builder = WebApplication.CreateBuilder(args);
if (!isTestHostProcess)
{
    builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);
}
builder.Configuration.AddEnvironmentVariables();

// Ensure default listening URL is port 5080 for local Kestrel runs, without overriding IIS/hosting ports
var hasHostBoundUrl = !string.IsNullOrWhiteSpace(builder.Configuration["Urls"])
    || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS"))
    || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"))
    || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_IIS_PHYSICAL_PATH"));
if (!hasHostBoundUrl)
{
    builder.WebHost.UseUrls("http://127.0.0.1:5080");
}
builder.WebHost.ConfigureKestrel(k => k.AddServerHeader = false);

// 1. Dual-Mode Database Persistence:
// Uses PostgreSQL (Supabase / Docker / self-hosted) when configured & reachable,
// never touching remote cloud DB when running under automated xUnit tests.
bool isRunningUnderTests = isTestHostProcess
                            || !string.IsNullOrWhiteSpace(builder.Configuration["Database:InMemoryName"]);

var usePostgresSetting = Environment.GetEnvironmentVariable("SWMS_USE_POSTGRES")
    ?? builder.Configuration["Database:UsePostgres"];
var rawPostgresConn = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Postgres");
var postgresConn = NormalizePostgresConnectionString(rawPostgresConn);
bool isPostgresAvailable = false;

if (!isRunningUnderTests && !string.IsNullOrWhiteSpace(postgresConn))
{
    if (string.Equals(usePostgresSetting, "true", StringComparison.OrdinalIgnoreCase))
    {
        isPostgresAvailable = true;
    }
    else if (!string.Equals(usePostgresSetting, "false", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var (pgHost, pgPort) = ExtractPostgresHostAndPort(postgresConn);
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(pgHost, pgPort);
            if (connectTask.Wait(TimeSpan.FromMilliseconds(2500)) && tcp.Connected)
            {
                isPostgresAvailable = true;
            }
        }
        catch
        {
            isPostgresAvailable = false;
        }
    }
}

var activeDatabaseMode = isRunningUnderTests
    ? "Test (InMemory/SQLite)"
    : (isPostgresAvailable && !string.IsNullOrWhiteSpace(postgresConn)
        ? (postgresConn.Contains("supabase.com", StringComparison.OrdinalIgnoreCase) ? "PostgreSQL (Supabase)" : "PostgreSQL")
        : "SQLite");

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
        options.UseSqlite(sqliteConn)
               .AddInterceptors(new SqlitePragmaConnectionInterceptor());
    }
});

// 2. Security & Auth Services (JWT + AES-256-GCM RTR + Token Version Verification)
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new AesGcmTokenProtector(JwtTokenService.ResolveAesMasterKeyBase64(cfg));
});
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<RefreshTokenRotationService>();
builder.Services.AddHttpClient("GoogleAuth", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<IGoogleIdTokenValidator, GoogleJwksIdTokenValidator>();

var jwtSigningKey = JwtTokenService.ResolveJwtSigningKey(builder.Configuration);
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

                var user = !isRunningUnderTests ? JwtTokenService.GetCachedUser(userId) : null;
                if (user == null)
                {
                    var db = ctx.HttpContext.RequestServices.GetRequiredService<SwmsDbContext>();
                    user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null && !isRunningUnderTests)
                    {
                        JwtTokenService.SetCachedUser(user);
                    }
                }
                if (user == null || user.TokenVersion != tokenVer)
                {
                    JwtTokenService.InvalidateCachedUser(userId);
                    ctx.HttpContext.Items["JwtVerRevoked"] = "TOKEN_VERSION_REVOKED";
                    ctx.Fail("JWT token_version has been revoked.");
                    return;
                }
                ctx.HttpContext.Items["AuthenticatedUser"] = user;
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
var configuredOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var allowedOrigins = new[] { "http://localhost:3000", "http://127.0.0.1:3000" }
    .Concat(configuredOrigins)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SwmsFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; frame-ancestors 'self'");
    ctx.Response.Headers.Remove("Server");
    await next();
});

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
app.Logger.LogInformation("SWMS Backend starting in database mode: {DatabaseMode}", activeDatabaseMode);
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();
    if (db.Database.IsNpgsql())
    {
        try
        {
            var hasUsersTable = (await db.Database
                .SqlQueryRaw<int>("SELECT COUNT(*)::int AS \"Value\" FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'users'")
                .ToListAsync())
                .FirstOrDefault() > 0;
            if (!hasUsersTable)
            {
                await db.Database.EnsureCreatedAsync();
                hasUsersTable = (await db.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*)::int AS \"Value\" FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'users'")
                    .ToListAsync())
                    .FirstOrDefault() > 0;
                if (!hasUsersTable)
                {
                    var creator = db.Database.GetService<IRelationalDatabaseCreator>();
                    await creator.CreateTablesAsync();
                }
            }

            var forceStartupSeed = string.Equals(
                Environment.GetEnvironmentVariable("SWMS_FORCE_STARTUP_SEED") ?? builder.Configuration["Database:ForceStartupSeed"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            var hasAnyUser = await db.Users.AsNoTracking().AnyAsync();
            var existingCourseCount = hasAnyUser ? await db.Courses.AsNoTracking().CountAsync() : 0;

            if (forceStartupSeed || !hasAnyUser || existingCourseCount < 14)
            {
                await SwmsEndpoints.EnsureSqliteSchemaUpdatedAsync(db);
                await SeedDataInitializer.SeedAsync(db, builder.Configuration["DocumentLibrary:RootPath"]);
            }
            else
            {
                app.Logger.LogInformation(
                    "Fast cold-start: PostgreSQL already has {CourseCount} courses. Running lightweight schema check in background.",
                    existingCourseCount);
                var rootScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
                var startupLogger = app.Logger;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var bgScope = rootScopeFactory.CreateScope();
                        var bgDb = bgScope.ServiceProvider.GetRequiredService<SwmsDbContext>();
                        await SwmsEndpoints.EnsureSqliteSchemaUpdatedAsync(bgDb);
                    }
                    catch (Exception bgEx)
                    {
                        startupLogger.LogWarning(bgEx, "Background PostgreSQL schema check warning.");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "PostgreSQL startup initialization warning; continuing startup so IIS/Kestrel health probes do not time out.");
        }
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
        await SwmsEndpoints.EnsureSqliteSchemaUpdatedAsync(db);
        await SeedDataInitializer.SeedAsync(db, builder.Configuration["DocumentLibrary:RootPath"]);
    }
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "SWMS Backend .NET 10 LTS"
}));

app.MapSwmsEndpoints();

app.Run();

public partial class Program
{
    private static string NormalizePostgresConnectionString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var trimmed = raw.Trim();

        NpgsqlConnectionStringBuilder csb;
        if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var schemeEnd = trimmed.IndexOf("://", StringComparison.Ordinal);
            var rest = trimmed[(schemeEnd + 3)..];
            var queryIdx = rest.IndexOf('?');
            var queryPart = queryIdx >= 0 ? rest[(queryIdx + 1)..] : string.Empty;
            if (queryIdx >= 0) rest = rest[..queryIdx];

            var slashIdx = rest.IndexOf('/');
            var authority = slashIdx >= 0 ? rest[..slashIdx] : rest;
            var database = slashIdx >= 0 && slashIdx + 1 < rest.Length
                ? Uri.UnescapeDataString(rest[(slashIdx + 1)..])
                : "postgres";

            var atIdx = authority.LastIndexOf('@');
            string? username = null;
            string? password = null;
            var hostPort = authority;
            if (atIdx >= 0)
            {
                var userInfo = authority[..atIdx];
                hostPort = authority[(atIdx + 1)..];
                var colonUserIdx = userInfo.IndexOf(':');
                if (colonUserIdx >= 0)
                {
                    username = Uri.UnescapeDataString(userInfo[..colonUserIdx]);
                    password = Uri.UnescapeDataString(userInfo[(colonUserIdx + 1)..]);
                }
                else
                {
                    username = Uri.UnescapeDataString(userInfo);
                }
            }

            var host = hostPort;
            var port = 5432;
            var colonHostIdx = hostPort.LastIndexOf(':');
            if (colonHostIdx > 0 && int.TryParse(hostPort[(colonHostIdx + 1)..], out var parsedPort))
            {
                host = hostPort[..colonHostIdx];
                port = parsedPort;
            }

            csb = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = string.IsNullOrWhiteSpace(database) ? "postgres" : database,
                Pooling = true,
                Timeout = 20,
                CommandTimeout = 120,
                KeepAlive = 30
            };
            if (!string.IsNullOrEmpty(username)) csb.Username = username;
            if (!string.IsNullOrEmpty(password)) csb.Password = password;

            if (!string.IsNullOrWhiteSpace(queryPart))
            {
                foreach (var pair in queryPart.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var eq = pair.IndexOf('=');
                    if (eq <= 0) continue;
                    var k = Uri.UnescapeDataString(pair[..eq]).Trim().ToLowerInvariant();
                    var v = Uri.UnescapeDataString(pair[(eq + 1)..]).Trim();
                    if (k == "sslmode" && Enum.TryParse<SslMode>(v, true, out var parsedSsl))
                    {
                        csb.SslMode = parsedSsl;
                    }
                }
            }
        }
        else
        {
            csb = new NpgsqlConnectionStringBuilder(trimmed);
        }

        if (!string.IsNullOrWhiteSpace(csb.Host) &&
            csb.Host.Contains("supabase.com", StringComparison.OrdinalIgnoreCase) &&
            csb.SslMode == SslMode.Disable)
        {
            csb.SslMode = SslMode.Require;
        }
        else if (!string.IsNullOrWhiteSpace(csb.Host) &&
                 csb.Host.Contains("supabase.com", StringComparison.OrdinalIgnoreCase) &&
                 csb.SslMode == SslMode.Prefer)
        {
            csb.SslMode = SslMode.Require;
        }

        return csb.ConnectionString;
    }

    private static (string Host, int Port) ExtractPostgresHostAndPort(string connStr)
    {
        try
        {
            var csb = new NpgsqlConnectionStringBuilder(connStr);
            return (string.IsNullOrWhiteSpace(csb.Host) ? "127.0.0.1" : csb.Host, csb.Port > 0 ? csb.Port : 5432);
        }
        catch
        {
            return ("127.0.0.1", 5432);
        }
    }
}

public sealed class SqlitePragmaConnectionInterceptor : DbConnectionInterceptor
{
    private const string PragmaSql =
        "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA temp_store=MEMORY; PRAGMA busy_timeout=5000; PRAGMA wal_autocheckpoint=20000; PRAGMA cache_size=-64000;";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = PragmaSql;
        cmd.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = PragmaSql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
