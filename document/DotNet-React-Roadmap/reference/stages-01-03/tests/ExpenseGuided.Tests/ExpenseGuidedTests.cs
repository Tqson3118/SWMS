using ExpenseGuided;

namespace ExpenseGuided.Tests;

public sealed class ExpenseGuidedTests
{
    [Fact]
    public void Expense_rejects_empty_category_and_negative_amount()
    {
        Assert.Throws<ArgumentException>(() => new Expense(" ", 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Expense("Điện", -1));
    }

    [Fact]
    public void Service_empty_total_and_category_totals_are_explicit()
    {
        var empty = new ExpenseService();
        Assert.Equal(0, empty.Total());
        Assert.Empty(empty.TotalsByCategory());
        empty.Add("Điện", 0);
        empty.Add("điện", 120);
        empty.Add("Ăn", 80);
        Assert.Equal(200, empty.Total());
        Assert.Equal(120, empty.TotalsByCategory()["Điện"]);
        Assert.Equal(80, empty.TotalsByCategory()["Ăn"]);
    }

    [Theory]
    [InlineData("100", true, 100)]
    [InlineData("0", true, 0)]
    [InlineData("12.5", true, 12.5)]
    [InlineData("-1", false, 0)]
    [InlineData("1,000", false, 0)]
    [InlineData("text", false, 0)]
    [InlineData("", false, 0)]
    public void Text_reader_enforces_plain_vnd_decimal_policy(string input, bool expected, decimal amount)
    {
        using var reader = new StringReader(input + "\n");
        using var writer = new StringWriter();
        Assert.Equal(expected, ConsoleInput.TryReadNonNegativeAmount(reader, writer, out decimal actual));
        Assert.Equal(amount, actual);
    }

    [Fact]
    public void Text_reader_handles_eof_without_looping()
    {
        using var reader = new StringReader(string.Empty);
        using var writer = new StringWriter();
        Assert.False(ConsoleInput.TryReadNonNegativeAmount(reader, writer, out _, out bool eof));
        Assert.True(eof);
        Assert.Contains("kết thúc input", writer.ToString());
    }

    [Fact]
    public void Runner_retries_invalid_amount_preserves_category_and_accepts_exactly_three_valid_entries()
    {
        using var reader = new StringReader("A\ntext\n5\nB\n0\nC\n2\n");
        using var writer = new StringWriter();
        ConsoleRunner.Run(reader, writer);
        string output = writer.ToString();
        Assert.Contains("Hãy nhập một số tiền không âm.", output);
        Assert.Contains("Đã thêm: A = 5", output);
        Assert.Contains("Đã thêm: B = 0", output);
        Assert.Contains("Đã thêm: C = 2", output);
        Assert.Contains("Tổng: 7", output);
    }

    [Fact]
    public void Runner_retries_invalid_category_and_does_not_add_invalid_entry()
    {
        using var reader = new StringReader(" \nValid\n10\nNext\n1\nLast\n2\n");
        using var writer = new StringWriter();
        ConsoleRunner.Run(reader, writer);
        string output = writer.ToString();
        Assert.Contains("Danh mục không được rỗng.", output);
        Assert.Contains("Tổng: 13", output);
        Assert.DoesNotContain("Đã thêm:  =", output);
    }

    [Theory]
    [InlineData("", "danh mục")]
    [InlineData("A\n", "số tiền")]
    public void Runner_stops_cleanly_at_category_or_amount_eof(string input, string expectedPrompt)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();
        ConsoleRunner.Run(reader, writer);
        Assert.Contains(expectedPrompt, writer.ToString());
        Assert.Contains("Đã kết thúc input.", writer.ToString());
    }

    [Fact]
    public void Json_store_round_trips_and_keeps_corrupt_json()
    {
        string path = RetainedTestPath("round-trip.json");
        var store = new JsonExpenseStore();
        var source = new[] { new Expense("Điện", 12.5m), new Expense("Ăn", 0) };
        store.Save(path, source);
        Assert.Equal(source, store.Load(path));
        File.WriteAllText(path, "{ broken");
        byte[] before = File.ReadAllBytes(path);
        Assert.Throws<InvalidDataException>(() => store.Load(path));
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Theory]
    [InlineData("[null]")]
    [InlineData("[{\"Category\":\"\",\"Amount\":1}]")]
    [InlineData("[{\"Category\":\"A\",\"Amount\":-1}]")]
    [InlineData("null")]
    [InlineData("{\"Category\":\"A\",\"Amount\":1}")]
    [InlineData("{ broken")]
    public void Json_store_rejects_null_invalid_invariants_and_wrong_roots_without_replacing_bytes(string json)
    {
        string path = RetainedTestPath("invalid.json");
        File.WriteAllText(path, json);
        byte[] before = File.ReadAllBytes(path);
        Assert.Throws<InvalidDataException>(() => new JsonExpenseStore().Load(path));
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    private static string RetainedTestPath(string fileName)
    {
        string directory = Path.Combine(Path.GetTempPath(), "expense-guided-retained", "validation720", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }
}
