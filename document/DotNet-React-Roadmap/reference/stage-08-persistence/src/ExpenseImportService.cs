using AsyncReference;
using Microsoft.EntityFrameworkCore;

namespace ExpenseImportPersistence;

public sealed record ImportOutcome(string Status, int ReadRows, int AcceptedRows, int SkippedDuplicates, string? ErrorCode = null);

public sealed class ImportHooks
{
    public Func<int, ValueTask>? AfterSourceRow { get; init; }
    public int? FaultAfterSourceRow { get; init; }
    public Action<string>? DiagnosticSink { get; init; }
    public Func<CancellationToken, ValueTask>? BeforeCommit { get; init; }
    public Func<ValueTask>? AfterCommit { get; init; }
}

public sealed class ExpenseImportService(IDbContextFactory<ImportDbContext> contextFactory)
{
    public async Task<ImportOutcome> ImportAsync(string path, CsvImportOptions options, CancellationToken cancellationToken, ImportHooks? hooks = null)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (options.AllowedCategoryIds is null || options.AllowedCategoryIds.Count == 0) throw new ArgumentException("Allowed categories are required.", nameof(options));
        if (options.MaximumAmount < 0 || options.MaximumAmount > 9999999999999999.99m || decimal.GetBits(options.MaximumAmount)[3] / 65536 % 256 > 2)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum amount is outside the supported SQL decimal bound.");
        if (options.MaximumNoteLength < 0 || options.MaximumNoteLength > 500) throw new ArgumentOutOfRangeException(nameof(options), "Maximum note length is outside the supported bound.");
        cancellationToken.ThrowIfCancellationRequested();
        FileStream lease;
        try { lease = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            EmitDiagnostic(hooks, "input-open-failed");
            throw new InvalidDataException("Import input is unavailable.");
        }
        await using (lease)
        {
            if (lease.Length > 1024 * 1024) throw new InvalidDataException("Import file exceeds the 1 MiB limit.");
            var initialLength = lease.Length;
            var parsed = await CsvExpenseImporter.ImportFileAsync(path, options, cancellationToken,
                afterSourceRow: hooks?.AfterSourceRow, faultAfterSourceRow: hooks?.FaultAfterSourceRow);
            try
            {
                if (lease.Length != initialLength || new FileInfo(path).Length != initialLength) throw new InvalidDataException("Import input changed while it was being read.");
            }
            catch (FileNotFoundException) { throw new InvalidDataException("Import input changed while it was being read."); }
            catch (DirectoryNotFoundException) { throw new InvalidDataException("Import input changed while it was being read."); }
        if (parsed.Status == "cancelled-before-read") throw new OperationCanceledException(cancellationToken);
        if (parsed.Status == "faulted") throw new InvalidDataException("Import read failed.");
        if (parsed.Status == "partial-output-retained") throw new OperationCanceledException(cancellationToken);
        if (parsed.SourceRows > 1000) throw new InvalidDataException("Import exceeds the 1000-row limit.");
        var nonDuplicateErrors = parsed.Errors.Where(error => error.Code != "duplicate").ToArray();
        if (parsed.Status == "invalid-header" || nonDuplicateErrors.Length > 0) throw new InvalidDataException("Import validation failed; no rows were written.");
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var categoryIds = parsed.Accepted.Select(x => x.CategoryId).Distinct().ToArray();
        var knownCategories = await db.Categories.Where(x => categoryIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (knownCategories.Count != categoryIds.Length) throw new InvalidDataException("Import contains an unknown category.");
        db.Expenses.AddRange(parsed.Accepted.Select(x => new ImportedExpense { CategoryId = x.CategoryId, Amount = x.Amount, SpentOn = x.SpentOn, Note = x.Note }));
        await db.SaveChangesAsync(cancellationToken);
        try { if (hooks?.BeforeCommit is not null) await hooks.BeforeCommit(cancellationToken); }
        catch (OperationCanceledException) { throw; }
        catch { EmitDiagnostic(hooks, "precommit-hook-failed"); throw new InvalidOperationException("Import failed before commit."); }
        cancellationToken.ThrowIfCancellationRequested();
        try { await transaction.CommitAsync(CancellationToken.None); }
        catch { EmitDiagnostic(hooks, "commit-outcome-unknown"); throw new InvalidOperationException("Import commit outcome is unknown."); }
        if (hooks?.AfterCommit is not null)
        {
            try { await hooks.AfterCommit(); }
            catch { EmitDiagnostic(hooks, "postcommit-hook-failed"); return new ImportOutcome("committed-with-postcommit-error", parsed.SourceRows, parsed.AcceptedRows, parsed.SkippedRows, "postcommit-hook-failed"); }
        }
        return new ImportOutcome("committed", parsed.SourceRows, parsed.AcceptedRows, parsed.SkippedRows);
        }
        catch (InvalidDataException) { throw; }
        catch (OperationCanceledException) { throw; }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { EmitDiagnostic(hooks, "persistence-failed"); throw new InvalidOperationException("Import persistence failed."); }
        catch (Microsoft.Data.SqlClient.SqlException) { EmitDiagnostic(hooks, "persistence-failed"); throw new InvalidOperationException("Import persistence failed."); }
        }
    }

    private static void EmitDiagnostic(ImportHooks? hooks, string code)
    {
        try { hooks?.DiagnosticSink?.Invoke(code); }
        catch { /* diagnostics are secondary; they cannot change the import outcome */ }
    }
}
