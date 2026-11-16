using System.Globalization;
using System.Runtime.CompilerServices;

namespace AsyncReference;

public sealed record CsvImportOptions(IReadOnlySet<int> AllowedCategoryIds, decimal MaximumAmount, int MaximumNoteLength = 500);

public sealed record CsvImportRow(int CategoryId, decimal Amount, DateOnly SpentOn, string Note);

public sealed record CsvImportError(int LineNumber, string Code, string Message);

public sealed record CsvImportReport(
    int SourceRows,
    int AcceptedRows,
    int SkippedRows,
    IReadOnlyList<CsvImportRow> Accepted,
    IReadOnlyList<CsvImportError> Errors,
    string Status);

public static class CsvExpenseImporter
{
    private const string Header = "categoryId,amount,spentOn,note";

    public static async Task<CsvImportReport> ImportFileAsync(
        string path,
        CsvImportOptions options,
        CancellationToken cancellationToken,
        Func<int, ValueTask>? afterSourceRow = null,
        int? faultAfterSourceRow = null)
    {
        var accepted = new List<CsvImportRow>();
        var errors = new List<CsvImportError>();
        var seen = new HashSet<(int CategoryId, decimal Amount, DateOnly SpentOn, string Note)>();
        var sourceRows = 0;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var first = true;
            await foreach (var line in ReadLinesAsync(path, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (first)
                {
                    first = false;
                    if (!string.Equals(line, Header, StringComparison.Ordinal))
                    {
                        errors.Add(new CsvImportError(1, "header", "CSV header is invalid; no data rows were read."));
                        return new CsvImportReport(0, 0, 0, accepted, errors, "invalid-header");
                    }
                    continue;
                }

                sourceRows++;
                if (TryParse(line, sourceRows + 1, options, seen, out var row, out var error))
                    accepted.Add(row!);
                else
                {
                    errors.Add(error!);
                }

                if (afterSourceRow is not null) await afterSourceRow(sourceRows);
                if (faultAfterSourceRow == sourceRows)
                    throw new IOException("Injected deterministic read fault for teaching.");
            }

            var status = errors.Count == 0 ? "complete" : "complete-with-errors";
            return new CsvImportReport(sourceRows, accepted.Count, sourceRows - accepted.Count, accepted, errors, status);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var status = sourceRows == 0 ? "cancelled-before-read" : "partial-output-retained";
            return new CsvImportReport(sourceRows, accepted.Count, sourceRows - accepted.Count, accepted, errors, status);
        }
        catch (Exception)
        {
            errors.Add(new CsvImportError(sourceRows + 1, "read-fault", "Input could not be read; details stay in the raw server log."));
            return new CsvImportReport(sourceRows, accepted.Count, sourceRows - accepted.Count, accepted, errors, "faulted");
        }
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
            yield return line;
    }

    private static bool TryParse(
        string line,
        int lineNumber,
        CsvImportOptions options,
        HashSet<(int CategoryId, decimal Amount, DateOnly SpentOn, string Note)> seen,
        out CsvImportRow? row,
        out CsvImportError? error)
    {
        row = null;
        if (!TrySplit(line, out var fields, out var splitError))
        {
            error = new CsvImportError(lineNumber, splitError, splitError == "unsupported-multiline"
                ? "Quoted fields cannot span physical lines."
                : "CSV quoting is malformed.");
            return false;
        }
        if (fields.Count != 4)
        {
            error = new CsvImportError(lineNumber, "missing-column", "Exactly four columns are required.");
            return false;
        }
        if (!int.TryParse(fields[0], NumberStyles.None, CultureInfo.InvariantCulture, out var categoryId) ||
            !options.AllowedCategoryIds.Contains(categoryId))
        {
            error = new CsvImportError(lineNumber, "category", "Category is not in the allowed catalog.");
            return false;
        }
        if (!decimal.TryParse(fields[1], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var amount) || amount < 0 || amount > options.MaximumAmount ||
            decimal.GetBits(amount)[3] / 65536 % 256 > 2)
        {
            error = new CsvImportError(lineNumber, "amount", "Amount must be invariant decimal with scale 0-2 in range.");
            return false;
        }
        if (!DateOnly.TryParseExact(fields[2], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var spentOn))
        {
            error = new CsvImportError(lineNumber, "date", "Date must be yyyy-MM-dd.");
            return false;
        }
        if (fields[3].Length > options.MaximumNoteLength)
        {
            error = new CsvImportError(lineNumber, "note", "Note exceeds the configured length bound.");
            return false;
        }
        if (!seen.Add((categoryId, amount, spentOn, fields[3])))
        {
            error = new CsvImportError(lineNumber, "duplicate", "The complete expense row is duplicated.");
            return false;
        }

        row = new CsvImportRow(categoryId, amount, spentOn, fields[3]);
        error = null;
        return true;
    }

    private static bool TrySplit(string line, out List<string> fields, out string errorCode)
    {
        fields = new List<string>();
        var field = new System.Text.StringBuilder();
        var quoted = false;
        var closedQuote = false;
        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (quoted)
            {
                if (character == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { field.Append('"'); i++; }
                    else { quoted = false; closedQuote = true; }
                }
                else field.Append(character);
            }
            else if (closedQuote && character == ',') { fields.Add(field.ToString()); field.Clear(); closedQuote = false; }
            else if (closedQuote) { errorCode = "malformed-quoting"; return false; }
            else if (character == ',') { fields.Add(field.ToString()); field.Clear(); }
            else if (character == '"' && field.Length == 0) quoted = true;
            else if (character == '"') { errorCode = "malformed-quoting"; return false; }
            else field.Append(character);
        }
        if (quoted) { errorCode = "unsupported-multiline"; return false; }
        fields.Add(field.ToString());
        errorCode = string.Empty;
        return true;
    }
}
