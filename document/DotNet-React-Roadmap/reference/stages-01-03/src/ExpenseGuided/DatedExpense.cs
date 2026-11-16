namespace ExpenseGuided;

public sealed record DatedExpense
{
    public DatedExpense(int id, string category, decimal amount, DateOnly spentOn)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Id phải là số nguyên dương.");
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Danh mục không được rỗng.", nameof(category));
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Số tiền không được âm.");
        Id = id;
        Category = category.Trim();
        Amount = amount;
        SpentOn = spentOn;
    }

    public int Id { get; }
    public string Category { get; }
    public decimal Amount { get; }
    public DateOnly SpentOn { get; }
}
