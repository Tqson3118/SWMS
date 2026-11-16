using AsyncReference;
using Xunit;

namespace AsyncReference.Tests;

public sealed class CsvImportTests
{
    private static readonly CsvImportOptions Options = new(new HashSet<int> { 1, 2, 7 }, 1_000_000m);

    [Fact]
    public async Task Csv_file_stream_counts_source_accepted_skipped_and_line_errors()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,12.50,2026-09-22,coffee\n2,-1.00,2026-09-23,bad\n9,2.00,2026-09-24,unknown\n1,3.333,2026-09-25,scale\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, CancellationToken.None);
        Assert.Equal(4, report.SourceRows);
        Assert.Equal(1, report.AcceptedRows);
        Assert.Equal(3, report.SkippedRows);
        Assert.Equal(new[] { 12.50m }, report.Accepted.Select(x => x.Amount));
        Assert.Equal(new[] { 3, 4, 5 }, report.Errors.Select(x => x.LineNumber));
        Assert.Equal("complete-with-errors", report.Status);
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Csv_parser_supports_quoted_commas_and_escaped_quotes_but_rejects_unclosed_multiline()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,\"coffee, hot\"\n2,2.00,2026-09-23,\"say \"\"hi\"\"\"\n1,3.00,2026-09-24,\"unclosed\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, CancellationToken.None);
        Assert.Equal(new[] { "coffee, hot", "say \"hi\"" }, report.Accepted.Select(x => x.Note));
        Assert.Contains(report.Errors, error => error.Code == "unsupported-multiline");
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Empty_and_header_only_files_have_zero_rows_and_are_complete()
    {
        var empty = NewCsv("");
        var header = NewCsv("categoryId,amount,spentOn,note\n");
        var emptyReport = await CsvExpenseImporter.ImportFileAsync(empty, Options, CancellationToken.None);
        var headerReport = await CsvExpenseImporter.ImportFileAsync(header, Options, CancellationToken.None);
        Assert.Equal((0, 0, 0), (emptyReport.SourceRows, emptyReport.AcceptedRows, emptyReport.SkippedRows));
        Assert.Equal((0, 0, 0), (headerReport.SourceRows, headerReport.AcceptedRows, headerReport.SkippedRows));
        AssertFileClosed(empty);
        AssertFileClosed(header);
    }

    [Fact]
    public async Task Precancelled_input_reads_no_file_rows_and_is_marked_cancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var path = NewCsv("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,a\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, cts.Token);
        Assert.Equal((0, 0, 0), (report.SourceRows, report.AcceptedRows, report.SkippedRows));
        Assert.Equal("cancelled-before-read", report.Status);
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Midread_cancellation_keeps_partial_rows_and_closes_actual_file_stream()
    {
        using var cts = new CancellationTokenSource();
        var path = NewCsv("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,a\n2,2.00,2026-09-23,b\n7,3.00,2026-09-24,c\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, cts.Token,
            afterSourceRow: rows => { if (rows == 2) cts.Cancel(); return ValueTask.CompletedTask; });
        Assert.Equal(2, report.SourceRows);
        Assert.Equal(new[] { 1m, 2m }, report.Accepted.Select(x => x.Amount));
        Assert.Equal("partial-output-retained", report.Status);
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Fault_midread_is_reported_and_actual_file_stream_is_closed()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,a\n2,2.00,2026-09-23,b\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, CancellationToken.None, faultAfterSourceRow: 1);
        Assert.Equal(1, report.SourceRows);
        Assert.Equal("faulted", report.Status);
        Assert.Contains(report.Errors, error => error.Code == "read-fault");
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Duplicate_category_date_and_missing_columns_are_skipped_with_explicit_errors()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,a\n1,1.00,2026-09-22,a\n1,3.00,2026-09-23\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, CancellationToken.None);
        Assert.Equal(3, report.SourceRows);
        Assert.Equal(1, report.AcceptedRows);
        Assert.Contains(report.Errors, error => error.Code == "duplicate");
        Assert.Contains(report.Errors, error => error.Code == "missing-column");
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Amount_uses_invariant_decimal_and_enforces_maximum_without_overflow()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,999999.99,2026-09-22,max\n2,1000000.00,2026-09-23,too-high\n7,\"1,25\",2026-09-24,comma-decimal\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options with { MaximumAmount = 999999.99m }, CancellationToken.None);
        Assert.Equal(3, report.SourceRows);
        Assert.Equal(1, report.AcceptedRows);
        Assert.Contains(report.Errors, error => error.Code == "amount");
        Assert.Contains(report.Errors, error => error.Code == "amount");
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Invalid_or_blank_header_stops_before_accepting_data()
    {
        var wrong = NewCsv("wrong,header\n1,1.00,2026-09-22,a\n");
        var blank = NewCsv("\ncategoryId,amount,spentOn,note\n1,1.00,2026-09-22,a\n");
        var wrongReport = await CsvExpenseImporter.ImportFileAsync(wrong, Options, CancellationToken.None);
        var blankReport = await CsvExpenseImporter.ImportFileAsync(blank, Options, CancellationToken.None);
        Assert.Equal((0, 0, 0), (wrongReport.SourceRows, wrongReport.AcceptedRows, wrongReport.SkippedRows));
        Assert.Equal("invalid-header", wrongReport.Status);
        Assert.Equal((0, 0, 0), (blankReport.SourceRows, blankReport.AcceptedRows, blankReport.SkippedRows));
        Assert.Equal("invalid-header", blankReport.Status);
        AssertFileClosed(wrong);
        AssertFileClosed(blank);
    }

    [Fact]
    public async Task Malformed_quotes_are_rejected_without_accepting_junk_after_closing_quote()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,\"x\"junk\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options, CancellationToken.None);
        Assert.Equal(1, report.SourceRows);
        Assert.Equal(0, report.AcceptedRows);
        Assert.Contains(report.Errors, error => error.Code == "malformed-quoting");
        AssertFileClosed(path);
    }

    [Fact]
    public async Task Invalid_date_overflow_and_note_limit_are_reported_without_detail_leak()
    {
        var path = NewCsv("categoryId,amount,spentOn,note\n1,79228162514264337593543950336,2026-09-22,overflow\n1,1.00,2026-02-30,bad-date\n1,1.00,2026-09-22,12345\n");
        var report = await CsvExpenseImporter.ImportFileAsync(path, Options with { MaximumNoteLength = 4 }, CancellationToken.None);
        Assert.Equal(3, report.SourceRows);
        Assert.Equal(0, report.AcceptedRows);
        Assert.Contains(report.Errors, error => error.Code == "amount");
        Assert.Contains(report.Errors, error => error.Code == "date");
        Assert.Contains(report.Errors, error => error.Code == "note");
        Assert.DoesNotContain(report.Errors, error => error.Message.Contains(path, StringComparison.OrdinalIgnoreCase));
        AssertFileClosed(path);
    }

    private static string NewCsv(string content)
    {
        var root = Environment.GetEnvironmentVariable("STAGE8_FIXTURE_DIR") ??
            Path.Combine(Path.GetTempPath(), "stage8-async-correct899-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "fixture-" + Guid.NewGuid().ToString("N") + ".csv");
        File.WriteAllText(path, content);
        return path;
    }

    private static void AssertFileClosed(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        Assert.True(stream.CanRead);
    }
}
