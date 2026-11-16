namespace AsyncReference;

public sealed class TrackingResource : IAsyncDisposable
{
    public bool Disposed { get; private set; }
    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}

public sealed record ImportReport(
    int ReadRows,
    IReadOnlyList<decimal> AcceptedAmounts,
    bool Cancelled,
    string Status);

public static class ExpenseImporter
{
    public static async Task<ImportReport> ImportAsync(
        IEnumerable<string> lines,
        CancellationToken cancellationToken,
        IAsyncDisposable resource,
        Func<int, ValueTask>? afterRead = null)
    {
        var accepted = new List<decimal>();
        var readRows = 0;
        await using (resource)
        {
            try
            {
                foreach (var line in lines)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!decimal.TryParse(line, out var amount) || amount < 0) continue;
                    accepted.Add(amount);
                    readRows++;
                    if (afterRead is not null) await afterRead(readRows);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new ImportReport(readRows, accepted, true, "partial-output-retained");
            }
        }

        return new ImportReport(readRows, accepted, false, "complete");
    }
}

public sealed class OwnedCounter
{
    private readonly object gate = new();
    private int value;
    public int Value { get { lock (gate) return value; } }
    public void Add(int amount) { lock (gate) value += amount; }
}
