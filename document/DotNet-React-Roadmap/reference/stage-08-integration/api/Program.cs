using System.Net;
using System.Globalization;
using System.Text.Json.Serialization;
using ExpenseImportPersistence;
using AsyncReference;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
ApiContract.ValidateEndpointOverrides(builder.Configuration);
var listenUrl = ApiContract.ValidateListenUrl(Environment.GetEnvironmentVariable("DN54_LISTEN_URL") ?? ApiContract.DefaultListenUrl);
builder.WebHost.UseKestrel().UseUrls(listenUrl);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = ApiContract.MaxRequestBytes);
var databaseName = Environment.GetEnvironmentVariable("DN54_DB_NAME") ?? throw new InvalidOperationException("DN54_DB_NAME is required.");
var fixtureRoot = Environment.GetEnvironmentVariable("DN54_FIXTURE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-api");
Directory.CreateDirectory(fixtureRoot);
var connectionString = ImportDatabase.ConnectionString(databaseName);
var dbOptions = new DbContextOptionsBuilder<ImportDbContext>().UseSqlServer(connectionString).Options;
builder.Services.AddSingleton<IDbContextFactory<ImportDbContext>>(_ => new PooledDbContextFactory<ImportDbContext>(dbOptions));
builder.Services.AddSingleton(new ExpenseImportService(new PooledDbContextFactory<ImportDbContext>(dbOptions)));
builder.Services.AddSingleton(new ImportHooks());
builder.Services.AddSingleton(new ImportRequestHooks());
var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine($"DN54_LISTENING={string.Join(',', app.Urls)}"));
app.MapGet("/api/v1/expenses", async (IDbContextFactory<ImportDbContext> factory, CancellationToken ct) =>
{
    await using var db = await factory.CreateDbContextAsync(ct);
    var rows = await db.Expenses.AsNoTracking().OrderBy(x => x.Id).Select(x => new ExpenseDto(x.Id, x.CategoryId, x.Amount.ToString("0.00", CultureInfo.InvariantCulture), x.SpentOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), x.Note)).ToListAsync(ct);
    return Results.Ok(rows);
});
app.MapPost("/api/v1/expenses/import", async (HttpRequest request, ExpenseImportService service, ImportHooks hooks, ImportRequestHooks requestHooks, CancellationToken ct) =>
{
    var traceId = request.HttpContext.TraceIdentifier;
    try
    {
        if (!request.HasFormContentType) return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "multipart_required", "Use multipart/form-data with a file field.")], traceId));
        if (requestHooks.BeforeUpload is not null) await requestHooks.BeforeUpload(ct);
        ct.ThrowIfCancellationRequested();
        IFormCollection form;
        try { form = await request.ReadFormAsync(ct); }
        catch (InvalidDataException) { return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "invalid_multipart", "The multipart request is invalid.")], traceId)); }
        var upload = form.Files.GetFile("file");
        if (upload is null) return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "file_required", "A file field is required.")], traceId));
        var fixture = Path.Combine(fixtureRoot, $"upload-{Guid.NewGuid():N}.csv");
        await using (var output = new FileStream(fixture, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        await using (var input = upload.OpenReadStream())
        {
            var buffer = new byte[81920]; var total = 0L; int read;
            while ((read = await input.ReadAsync(buffer, ct)) > 0) { total += read; if (total > ApiContract.MaxFileBytes) return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "file_too_large", "The file exceeds the 1 MiB limit.")], traceId)); await output.WriteAsync(buffer.AsMemory(0, read), ct); if (requestHooks.AfterUploadBytes is not null) await requestHooks.AfterUploadBytes(total, ct); ct.ThrowIfCancellationRequested(); }
        }
        if (requestHooks.AfterRetain is not null) await requestHooks.AfterRetain(fixture, ct);
        ct.ThrowIfCancellationRequested();
        if (new FileInfo(fixture).Length > ApiContract.MaxFileBytes) return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "file_too_large", "The retained file exceeds the 1 MiB limit.")], traceId));
        var options = new CsvImportOptions(new HashSet<int> { 1, 2, 7 }, ApiContract.MaxAmount, 500);
        var parsed = await CsvExpenseImporter.ImportFileAsync(fixture, options, ct);
        var nonDuplicate = parsed.Errors.Where(x => x.Code != "duplicate").ToArray();
        if (parsed.Status == "invalid-header" || parsed.Status == "faulted" || nonDuplicate.Length > 0 || parsed.SourceRows > 1000)
        {
            var code = parsed.SourceRows > 1000 ? "row_limit" : parsed.Status == "faulted" ? "read_fault" : parsed.Status == "invalid-header" ? "header" : "invalid_row";
            var errors = parsed.Errors.Select(x => new SafeError(x.LineNumber, x.Code, x.Code == "read-fault" ? "The retained input could not be read." : x.Message)).ToArray();
            if (errors.Length == 0) errors = [new SafeError(null, code, code == "row_limit" ? "The file exceeds the 1000-row limit." : "The import was rejected.")];
            return Results.BadRequest(new ImportResponse("rejected", parsed.SourceRows, 0, parsed.SkippedRows, errors, traceId));
        }
        var result = await service.ImportAsync(fixture, options, ct, hooks);
        return Results.Ok(new ImportResponse(result.Status, result.ReadRows, result.AcceptedRows, result.SkippedDuplicates, result.ErrorCode is null ? [] : [new SafeError(null, result.ErrorCode, "The rows were committed; a follow-up step failed.")], traceId));
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested) { return Results.Json(new ImportResponse("cancelled", 0, 0, 0, [], traceId), statusCode: 499); }
    catch (BadHttpRequestException error) { return Results.Json(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, error.StatusCode == 413 ? "request_too_large" : "invalid_request", error.StatusCode == 413 ? "The whole request exceeds the allowed size." : "The request cannot be read.")], traceId), statusCode: error.StatusCode == 413 ? 413 : 400); }
    catch (IOException) { return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "input_unavailable", "The upload could not be read or retained.")], traceId)); }
    catch (UnauthorizedAccessException) { return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "input_unavailable", "The upload could not be read or retained.")], traceId)); }
    catch (InvalidDataException) { return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "invalid_import", "The input could not be imported; no rows were written.")], traceId)); }
    catch (ArgumentException) { return Results.BadRequest(new ImportResponse("rejected", 0, 0, 0, [new SafeError(null, "invalid_request", "The request is invalid.")], traceId)); }
    catch (InvalidOperationException) { return Results.Json(new ImportResponse("unknown", null, null, null, [new SafeError(null, "outcome_unknown", "The import outcome could not be confirmed. Refresh before deciding what to do next.")], traceId), statusCode: 500); }
});
await ImportDatabase.CreateFreshAsync(databaseName, Path.Combine(fixtureRoot, $"database-{Guid.NewGuid():N}.log"));
await using (var db = new ImportDbContext(dbOptions)) await db.Database.MigrateAsync();
await app.RunAsync();

public static class ApiContract
{
    public const string DefaultListenUrl = "http://127.0.0.1:0";
    public const long MaxFileBytes = 1024 * 1024;
    public const long MaxRequestBytes = MaxFileBytes + 64 * 1024;
    public const decimal MaxAmount = 9999999999999999.99m;
    public static void ValidateEndpointOverrides(IConfiguration config)
    {
        if (config.GetSection("Kestrel:Endpoints").GetChildren().Any())
            throw new InvalidOperationException("Kestrel endpoint overrides are not supported in this local fixture.");
    }
    public static string ValidateListenUrl(string raw)
    {
        if (raw != DefaultListenUrl && raw != DefaultListenUrl + "/")
            throw new InvalidOperationException("The API must bind to http://127.0.0.1:0.");
        return $"http://127.0.0.1:0";
    }
}
public sealed record ImportResponse(string Status, int? ReadRows, int? AcceptedRows, int? SkippedDuplicates, IReadOnlyList<SafeError> Errors, string TraceId);
public sealed record SafeError(int? LineNumber, string Code, string Message);
public sealed record ExpenseDto(long Id, int CategoryId, string Amount, string SpentOn, string Note);

public partial class Program { }

// Instructor-only DI seams. No HTTP parameter can choose or enable these callbacks.
public sealed class ImportRequestHooks
{
    public Func<CancellationToken, ValueTask>? BeforeUpload { get; init; }
    public Func<long, CancellationToken, ValueTask>? AfterUploadBytes { get; init; }
    public Func<string, CancellationToken, ValueTask>? AfterRetain { get; init; }
}
