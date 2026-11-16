using ExpenseGuided;

namespace ExpenseGuided.Tests;

public sealed class DatedExpenseTests
{
    [Fact]
    public void Domain_requires_positive_id_nonempty_category_nonnegative_amount_and_keeps_date()
    {
        var item = new DatedExpense(7, " Điện ", 12.5m, new DateOnly(2026, 2, 28));
        Assert.Equal(7, item.Id); Assert.Equal("Điện", item.Category); Assert.Equal(12.5m, item.Amount); Assert.Equal(new DateOnly(2026, 2, 28), item.SpentOn);
        Assert.Throws<ArgumentOutOfRangeException>(() => new DatedExpense(0, "A", 1, new DateOnly(2026, 1, 1)));
        Assert.Throws<ArgumentException>(() => new DatedExpense(1, " ", 1, new DateOnly(2026, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DatedExpense(1, "A", -1, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Service_rejects_duplicate_ids_lists_removes_and_separates_months()
    {
        var service = new DatedExpenseService();
        service.Add(new DatedExpense(1, "A", 10, new DateOnly(2026, 1, 31)));
        service.Add(new DatedExpense(2, "B", 4, new DateOnly(2026, 2, 1)));
        Assert.Throws<InvalidOperationException>(() => service.Add(new DatedExpense(1, "C", 1, new DateOnly(2026, 3, 1))));
        Assert.Equal(10, service.TotalForMonth(2026, 1)); Assert.Equal(4, service.TotalForMonth(2026, 2)); Assert.Equal(0, service.TotalForMonth(2025, 1));
        Assert.False(service.Remove(99)); Assert.True(service.Remove(1)); Assert.Single(service.List());
    }

    [Fact]
    public void Service_failed_update_preserves_previous_valid_value_and_unknown_update_is_false()
    {
        var service = new DatedExpenseService(); service.Add(new DatedExpense(3, "A", 8, new DateOnly(2026, 4, 5)));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Update(3, "A", -1, new DateOnly(2026, 4, 6)));
        Assert.Equal(new DatedExpense(3, "A", 8, new DateOnly(2026, 4, 5)), service.List().Single());
        Assert.False(service.Update(99, "B", 1, new DateOnly(2026, 4, 6)));
    }

    [Fact]
    public void Json_store_round_trips_exact_identity_date_and_amount()
    {
        string path = RetainedPath("dated-roundtrip.json"); var source = new[] { new DatedExpense(11, "A", 12.50m, new DateOnly(2026, 12, 31)) };
        var store = new DatedJsonExpenseStore(); store.Save(path, source);
        Assert.Equal(source, store.Load(path));
    }

    [Theory]
    [InlineData("[]", "empty list")]
    [InlineData("[{}]", "missing required")]
    [InlineData("[{\"Id\":1,\"Amount\":1,\"SpentOn\":\"2026-01-01\"}]", "missing category")]
    [InlineData("[{\"Id\":1,\"Category\":\"A\",\"Amount\":-1,\"SpentOn\":\"2026-01-01\"}]", "negative")]
    [InlineData("[{\"Id\":1,\"Category\":\"A\",\"Amount\":1,\"SpentOn\":\"2026-02-30\"}]", "invalid date")]
    [InlineData("[{\"Id\":1,\"Category\":\"A\",\"Amount\":1,\"SpentOn\":\"2026-01-01\"},{\"Id\":1,\"Category\":\"B\",\"Amount\":2,\"SpentOn\":\"2026-01-02\"}]", "duplicate")]
    [InlineData("null", "wrong root")]
    [InlineData("{ broken", "malformed")]
    public void Json_store_rejects_invalid_persisted_shape_and_preserves_bytes(string json, string _)
    {
        string path = RetainedPath("invalid-" + Guid.NewGuid().ToString("N") + ".json"); File.WriteAllText(path, json); byte[] before = File.ReadAllBytes(path);
        if (json == "[]") { Assert.Empty(new DatedJsonExpenseStore().Load(path)); return; }
        Assert.Throws<InvalidDataException>(() => new DatedJsonExpenseStore().Load(path)); Assert.Equal(before, File.ReadAllBytes(path));
    }

    private static string RetainedPath(string name)
    { string directory = Path.Combine(Path.GetTempPath(), "expense-guided-retained", "identity727", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory); return Path.Combine(directory, name); }
}
