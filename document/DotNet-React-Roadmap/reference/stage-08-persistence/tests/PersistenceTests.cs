using AsyncReference;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace ExpenseImportPersistence.Tests;

public sealed class PersistenceTests
{
    private static readonly CsvImportOptions Options = new(new HashSet<int> { 1, 2, 7 }, 9999999999999999.99m);

    [Fact]
    public async Task Valid_import_applies_owned_schema_and_persists_exact_money_date_fk_and_note()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,9999999999999999.99,2026-09-22,maximum\n2,12.50,2026-09-23,travel\n");
        var result = await fixture.Service.ImportAsync(path, Options, CancellationToken.None);
        Assert.Equal("committed", result.Status);
        await using var db = await fixture.Factory.CreateDbContextAsync();
        var rows = await db.Expenses.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(9999999999999999.99m, rows[0].Amount);
        Assert.Equal(new DateOnly(2026, 9, 22), rows[0].SpentOn);
        Assert.Equal(1, rows[0].CategoryId);
        Assert.Equal("maximum", rows[0].Note);
    }

    [Fact]
    public async Task Exact_duplicates_are_the_only_errors_that_still_commit_unique_rows()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,a\n1,1.00,2026-09-22,a\n2,2.00,2026-09-23,b\n");
        var result = await fixture.Service.ImportAsync(path, Options, CancellationToken.None);
        Assert.Equal(2, result.AcceptedRows);
        Assert.Equal(1, result.SkippedDuplicates);
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(2, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Validation_error_rejects_every_row_before_sql_effect()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,ok\n9,2.00,2026-09-23,bad-category\n");
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(path, Options, CancellationToken.None));
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Precommit_fault_rolls_back_rows_and_postcommit_cancellation_reports_committed()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,4.00,2026-09-22,fault\n");
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.ImportAsync(path, Options, CancellationToken.None,
            new ImportHooks { BeforeCommit = _ => throw new InvalidOperationException("injected precommit fault") }));
        await using (var db = await fixture.Factory.CreateDbContextAsync()) Assert.Equal(0, await db.Expenses.CountAsync());
        using var cancellation = new CancellationTokenSource();
        var committed = await fixture.Service.ImportAsync(path, Options, cancellation.Token,
            new ImportHooks { AfterCommit = () => { cancellation.Cancel(); return ValueTask.CompletedTask; } });
        Assert.Equal("committed", committed.Status);
        await using var check = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(1, await check.Expenses.CountAsync());
    }

    [Fact]
    public async Task Cancellation_before_read_and_before_commit_leave_zero_rows()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,4.00,2026-09-22,cancel\n");
        using var before = new CancellationTokenSource(); before.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Service.ImportAsync(path, Options, before.Token));
        using var during = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Service.ImportAsync(path, Options, during.Token,
            new ImportHooks { BeforeCommit = _ => { during.Cancel(); return ValueTask.CompletedTask; } }));
        using var duringRead = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Service.ImportAsync(path, Options, duringRead.Token,
            new ImportHooks { AfterSourceRow = _ => { duringRead.Cancel(); return ValueTask.CompletedTask; } }));
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task File_and_row_caps_are_rejected_without_sql_effect()
    {
        await using var fixture = await Fixture.CreateAsync();
        var overRows = fixture.Write("categoryId,amount,spentOn,note\n" + string.Join("\n", Enumerable.Repeat("1,1.00,2026-09-22,x", 1001)));
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(overRows, Options, CancellationToken.None));
        var oversized = fixture.Write("categoryId,amount,spentOn,note\n" + new string('x', 1024 * 1024));
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(oversized, Options, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() => fixture.Service.ImportAsync(overRows, null!, CancellationToken.None));
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Caller_bounds_missing_file_and_null_categories_are_safe()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,ok\n");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.Service.ImportAsync(path, Options with { MaximumAmount = 1.001m }, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.Service.ImportAsync(path, Options with { MaximumNoteLength = 501 }, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(path + "missing", Options, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ImportAsync(path, new CsvImportOptions(null!, 1m), CancellationToken.None));
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Caller_stricter_amount_and_note_bounds_reject_without_sql_effect()
    {
        await using var fixture = await Fixture.CreateAsync();
        var amount = fixture.Write("categoryId,amount,spentOn,note\n1,1.01,2026-09-22,ok\n");
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(amount, Options with { MaximumAmount = 1.00m }, CancellationToken.None));
        var note = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22," + new string('n', 101) + "\n");
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(note, Options with { MaximumNoteLength = 100 }, CancellationToken.None));
        var sqlMaxNote = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22," + new string('n', 501) + "\n");
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(sqlMaxNote, Options, CancellationToken.None));
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Read_fault_and_attempted_growth_reject_without_partial_sql()
    {
        await using var fixture = await Fixture.CreateAsync();
        var fault = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,fault\n1,2.00,2026-09-23,second\n");
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(fault, Options, CancellationToken.None,
            new ImportHooks { FaultAfterSourceRow = 1 }));
        var growth = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,growth\n");
        var growthAttempted = false;
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(growth, Options, CancellationToken.None,
            new ImportHooks { AfterSourceRow = _ => { if (!growthAttempted) { growthAttempted = true; File.AppendAllText(growth, "1,2.00,2026-09-23,added\n"); } return ValueTask.CompletedTask; } }));
        Assert.True(growthAttempted);
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Postcommit_hook_failure_reports_committed_effects()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,3.00,2026-09-22,post\n");
        var outcome = await fixture.Service.ImportAsync(path, Options, CancellationToken.None,
            new ImportHooks { AfterCommit = () => throw new InvalidOperationException("private hook detail") });
        Assert.Equal("committed-with-postcommit-error", outcome.Status);
        Assert.Equal("postcommit-hook-failed", outcome.ErrorCode);
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(1, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Postcommit_diagnostic_failure_still_reports_committed_effects()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,3.50,2026-09-22,diagnostic\n");
        var outcome = await fixture.Service.ImportAsync(path, Options, CancellationToken.None,
            new ImportHooks
            {
                AfterCommit = () => throw new InvalidOperationException("private hook detail"),
                DiagnosticSink = _ => throw new InvalidOperationException("private diagnostic detail")
            });
        Assert.Equal("committed-with-postcommit-error", outcome.Status);
        Assert.Equal("postcommit-hook-failed", outcome.ErrorCode);
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(1, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task Database_guard_rejects_fixed_or_newline_names()
    {
        Assert.Throws<ArgumentException>(() => ImportDatabase.ConnectionString("RoadmapImport915_fixed"));
        Assert.Throws<ArgumentException>(() => ImportDatabase.ConnectionString("RoadmapImport915_00000000000000000000000000000000\n"));
        await Assert.ThrowsAsync<ArgumentException>(() => ImportDatabase.CreateFreshAsync("RoadmapImport915_00000000000000000000000000000000\n", Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".log")));
    }

    [Fact]
    public async Task Locked_input_returns_safe_error_and_private_diagnostic_code()
    {
        await using var fixture = await Fixture.CreateAsync();
        var path = fixture.Write("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,locked\n");
        var diagnostics = new List<string>();
        await using var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.ImportAsync(path, Options, CancellationToken.None,
            new ImportHooks { DiagnosticSink = diagnostics.Add }));
        Assert.DoesNotContain(path, error.ToString(), StringComparison.Ordinal);
        Assert.Contains("input-open-failed", diagnostics);
        await using var db = await fixture.Factory.CreateDbContextAsync();
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public required IDbContextFactory<ImportDbContext> Factory { get; init; }
        public required ExpenseImportService Service { get; init; }
        public required string Root { get; init; }
        public string Write(string content) { var path = Path.Combine(Root, Guid.NewGuid().ToString("N") + ".csv"); File.WriteAllText(path, content); return path; }
        public static async Task<Fixture> CreateAsync()
        {
            var root = Path.Combine(Environment.GetEnvironmentVariable("DN53_FIXTURE_ROOT") ?? Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var name = ImportDatabase.NewName();
            var log = Path.Combine(root, $"database-{Guid.NewGuid():N}.log");
            await ImportDatabase.CreateFreshAsync(name, log);
            var options = new DbContextOptionsBuilder<ImportDbContext>().UseSqlServer(ImportDatabase.ConnectionString(name), sql => sql.MigrationsAssembly(typeof(ImportDbContext).Assembly.FullName)).Options;
            await using (var db = new ImportDbContext(options)) { await db.Database.MigrateAsync(); }
            var factory = new PooledDbContextFactory<ImportDbContext>(options);
            return new Fixture { Factory = factory, Service = new ExpenseImportService(factory), Root = root };
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
