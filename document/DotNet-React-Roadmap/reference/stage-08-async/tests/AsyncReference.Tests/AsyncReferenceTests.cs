using AsyncReference;
using Xunit;

namespace AsyncReference.Tests;

public sealed class AsyncReferenceTests
{
    [Fact]
    public void ExpenseKey_records_compare_by_value_but_expense_entities_use_identity()
    {
        Assert.Equal(new ExpenseKey(2, new DateOnly(2026, 9, 22)), new ExpenseKey(2, new DateOnly(2026, 9, 22)));
        Assert.NotEqual(new ExpenseEntity(2, 100m), new ExpenseEntity(2, 100m));
    }

    [Fact]
    public void Deferred_query_reads_new_expense_until_snapshot_boundary()
    {
        var expenses = new List<ExpenseEntity> { new(1, 10m) };
        var query = ExpenseQueries.PositiveAmounts(expenses);
        expenses.Add(new ExpenseEntity(2, 50m));
        Assert.Equal(new[] { 10m, 50m }, query);
        var snapshot = query.ToList();
        expenses.Add(new ExpenseEntity(3, 90m));
        Assert.Equal(new[] { 10m, 50m }, snapshot);
    }

    [Fact]
    public void Generic_map_and_try_parse_keep_types_and_nullable_boundary_explicit()
    {
        Assert.Equal(new[] { "10.00", "50.00" }, ExpenseQueries.Map(new[] { 10m, 50m }, x => x.ToString("F2")));
        Assert.True(InputParsing.TryParseDate("2026-09-22", out var date));
        Assert.Equal(new DateOnly(2026, 9, 22), date);
        Assert.False(InputParsing.TryParseDate("not-a-date", out _));
    }

    [Fact]
    public async Task Async_resource_is_disposed_after_successful_stream()
    {
        var resource = new TrackingResource();
        var report = await ExpenseImporter.ImportAsync(new[] { "10.00", "20.00" }, CancellationToken.None, resource);
        Assert.Equal(2, report.ReadRows);
        Assert.False(report.Cancelled);
        Assert.True(resource.Disposed);
    }

    [Fact]
    public async Task Cancellation_before_read_keeps_zero_rows_and_disposes_resource()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var resource = new TrackingResource();
        var report = await ExpenseImporter.ImportAsync(new[] { "10.00" }, cancellation.Token, resource);
        Assert.Equal(0, report.ReadRows);
        Assert.Empty(report.AcceptedAmounts);
        Assert.True(report.Cancelled);
        Assert.True(resource.Disposed);
    }

    [Fact]
    public async Task Cancellation_mid_operation_retains_partial_output_deterministically()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = new TrackingResource();
        var report = await ExpenseImporter.ImportAsync(
            new[] { "10.00", "20.00", "30.00" }, cancellation.Token, resource,
            afterRead: rows =>
            {
                if (rows == 2) cancellation.Cancel();
                return ValueTask.CompletedTask;
            });
        Assert.Equal(2, report.ReadRows);
        Assert.Equal(new[] { 10m, 20m }, report.AcceptedAmounts);
        Assert.True(report.Cancelled);
        Assert.True(resource.Disposed);
        Assert.Equal("partial-output-retained", report.Status);
    }

    [Fact]
    public async Task Cancellation_after_all_rows_is_success_when_no_boundary_is_pending()
    {
        using var cancellation = new CancellationTokenSource();
        var report = await ExpenseImporter.ImportAsync(new[] { "10.00" }, cancellation.Token, new TrackingResource(),
            afterRead: _ => { cancellation.Cancel(); return ValueTask.CompletedTask; });
        Assert.Equal(1, report.ReadRows);
        Assert.False(report.Cancelled);
    }

    [Fact]
    public void Shared_counter_is_deterministic_without_sleep_or_await_inside_lock()
    {
        var counter = new OwnedCounter();
        Parallel.For(0, 20, _ => counter.Add(1));
        Assert.Equal(20, counter.Value);
    }
}
