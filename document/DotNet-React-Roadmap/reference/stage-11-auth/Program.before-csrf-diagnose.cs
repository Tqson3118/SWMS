using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var dbName = Environment.GetEnvironmentVariable("ROADMAP_AUTH_DB") ?? $"RoadmapAuth11_{Guid.NewGuid():N}";
if (!System.Text.RegularExpressions.Regex.IsMatch(dbName, "^RoadmapAuth11_[0-9a-f]{32}$")) throw new InvalidOperationException("Strict auth DB name required");
var cs = $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Integrated Security=True;TrustServerCertificate=True;";
Console.Error.WriteLine($"AUTH_DB_BEFORE_CREATE name={dbName} server=(localdb)\\MSSQLLocalDB integratedSecurity=true");
await RefuseExistingDatabaseAsync(dbName);
builder.Services.AddDbContext<AuthDb>(o => o.UseSqlServer(cs));
builder.Services.AddIdentityCore<LearningUser>(o => { o.Password.RequiredLength = 12; o.User.RequireUniqueEmail = true; }).AddEntityFrameworkStores<AuthDb>();
builder.Services.AddIdentity<LearningUser, IdentityRole>().AddEntityFrameworkStores<AuthDb>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; o.Cookie.SameSite = SameSiteMode.Strict; o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = 401; return Task.CompletedTask; }; o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = 403; return Task.CompletedTask; }; });
builder.Services.AddAntiforgery(o => { o.HeaderName = "X-CSRF-TOKEN"; o.Cookie.Name = "RoadmapAuth11.XSRF"; o.Cookie.HttpOnly = false; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; o.Cookie.SameSite = SameSiteMode.Strict; });
builder.Services.AddAuthorization();
var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope()) { await scope.ServiceProvider.GetRequiredService<AuthDb>().Database.EnsureCreatedAsync(); }
app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.UseAntiforgery(); app.Use(async (ctx, next) => { if ((ctx.Request.Method == "POST" || ctx.Request.Method == "PUT") && ctx.Request.Path.StartsWithSegments("/api/expenses")) { try { await ctx.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(ctx); } catch (Exception) { ctx.Response.StatusCode = 400; await ctx.Response.WriteAsJsonAsync(new { error = "csrf" }); return; } } await next(); });
app.MapGet("/api/csrf", (HttpContext http, IAntiforgery anti) => { var t = anti.GetAndStoreTokens(http); return Results.Ok(new { header = "X-CSRF-TOKEN", token = t.RequestToken }); });
app.MapPost("/api/register", async (RegisterRequest r, UserManager<LearningUser> users, SignInManager<LearningUser> signIn) => { var u = new LearningUser { UserName = r.Email, Email = r.Email }; var result = await users.CreateAsync(u, r.Password); if (!result.Succeeded) return Results.BadRequest(new { errors = result.Errors.Select(e => e.Code) }); await signIn.SignInAsync(u, false); return Results.Ok(new { userId = u.Id }); });
app.MapPost("/api/login", async (LoginRequest r, SignInManager<LearningUser> signIn) => { var result = await signIn.PasswordSignInAsync(r.Email, r.Password, false, false); return result.Succeeded ? Results.Ok() : Results.Unauthorized(); });
var expenses = app.MapGroup("/api/expenses").RequireAuthorization();
expenses.MapPost("", async (ExpenseRequest r, ClaimsPrincipal p, AuthDb db, CancellationToken ct) => { var owner = p.FindFirstValue(ClaimTypes.NameIdentifier); if (owner is null) return Results.Unauthorized(); if (r.Amount <= 0 || r.Amount > 1_000_000) return Results.BadRequest(new { error = "invalid amount" }); var e = new Expense { OwnerUserId = owner, Amount = r.Amount, Note = r.Note ?? "" }; db.Expenses.Add(e); await db.SaveChangesAsync(ct); return Results.Created($"/api/expenses/{e.Id}", new { e.Id, e.Amount, e.Note }); }).WithMetadata(new RequireAntiforgeryTokenAttribute(true));
expenses.MapGet("/{id:int}", async (int id, ClaimsPrincipal p, AuthDb db, CancellationToken ct) => { var owner = p.FindFirstValue(ClaimTypes.NameIdentifier); if (owner is null) return Results.Unauthorized(); var e = await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OwnerUserId == owner, ct); return e is null ? Results.NotFound() : Results.Ok(new { e.Id, e.Amount, e.Note }); });
expenses.MapPut("/{id:int}", async (int id, ExpenseRequest r, ClaimsPrincipal p, AuthDb db, CancellationToken ct) => { var owner = p.FindFirstValue(ClaimTypes.NameIdentifier); if (owner is null) return Results.Unauthorized(); var e = await db.Expenses.SingleOrDefaultAsync(x => x.Id == id && x.OwnerUserId == owner, ct); if (e is null) return Results.NotFound(); if (r.Amount <= 0 || r.Amount > 1_000_000) return Results.BadRequest(new { error = "invalid amount" }); e.Amount = r.Amount; e.Note = r.Note ?? ""; await db.SaveChangesAsync(ct); return Results.Ok(new { e.Id, e.Amount, e.Note }); }).WithMetadata(new RequireAntiforgeryTokenAttribute(true));
app.Run();
static async Task RefuseExistingDatabaseAsync(string dbName) { await using var c = new Microsoft.Data.SqlClient.SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;"); await c.OpenAsync(); await using var q = c.CreateCommand(); q.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name=@name"; q.Parameters.AddWithValue("@name", dbName); if ((int)(await q.ExecuteScalarAsync())! > 0) throw new InvalidOperationException("Refusing existing auth database"); }
public sealed record RegisterRequest([Required, EmailAddress] string Email, [Required, MinLength(12)] string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record ExpenseRequest(decimal Amount, string? Note, string? OwnerUserId);
public sealed class LearningUser : IdentityUser { }
public sealed class Expense { public int Id { get; set; } public string OwnerUserId { get; set; } = ""; public decimal Amount { get; set; } public string Note { get; set; } = ""; }
public sealed class AuthDb(DbContextOptions<AuthDb> o) : IdentityDbContext<LearningUser>(o) { public DbSet<Expense> Expenses => Set<Expense>(); protected override void OnModelCreating(ModelBuilder b) { base.OnModelCreating(b); b.Entity<Expense>().Property(x => x.Amount).HasPrecision(18,2); b.Entity<Expense>().HasIndex(x => new { x.OwnerUserId, x.Id }).IsUnique(); } }
public partial class Program { }











