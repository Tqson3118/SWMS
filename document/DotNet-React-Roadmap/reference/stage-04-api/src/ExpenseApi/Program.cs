using ExpenseApi.Data;
using ExpenseApi.Services;
using ExpenseSqlGuided;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size", "X-Total-Pages", "Location");
    });
});

string? configuredDbName = builder.Configuration["DatabaseName"];
if (string.IsNullOrWhiteSpace(configuredDbName))
{
    configuredDbName = Environment.GetEnvironmentVariable("EXPENSE_API_DB_NAME");
}

if (string.IsNullOrWhiteSpace(configuredDbName))
{
    configuredDbName = ApiDatabase.NewName();
}

ApiDatabase.ValidateFreshName(configuredDbName);
string databaseLogPath = Environment.GetEnvironmentVariable("DN45_API779_DB_LOG") ?? "";
await ApiDatabase.CreateFreshDatabaseAsync(configuredDbName, databaseLogPath);

builder.Services.AddDbContext<ExpenseDbContext>(options =>
    options.UseSqlServer(ApiDatabase.ConnectionString(configuredDbName)));

builder.Services.AddScoped<IExpenseService, EfExpenseService>();

var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception exception)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ApiErrors");
        logger.LogError(exception, "Unhandled API failure {TraceId} {Method} {Path}", context.TraceIdentifier, context.Request.Method, context.Request.Path);
        throw;
    }
});
app.UseCors("frontend");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();

app.Run();

public partial class Program { }
