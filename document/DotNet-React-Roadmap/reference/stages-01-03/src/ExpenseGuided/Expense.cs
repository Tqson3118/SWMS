namespace ExpenseGuided;

public sealed record Expense
{
    public Expense(string category, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Danh mục không được rỗng.", nameof(category));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Số tiền không được âm.");

        Category = category.Trim();
        Amount = amount;
    }

    public string Category { get; }
    public decimal Amount { get; }
}
