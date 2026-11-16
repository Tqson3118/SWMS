namespace ExpenseGuided;

public sealed class ExpenseService
{
    private readonly List<Expense> expenses = [];

    public IReadOnlyList<Expense> Items => expenses;

    public void Add(string category, decimal amount) => expenses.Add(new Expense(category, amount));

    public decimal Total() => expenses.Sum(expense => expense.Amount);

    public IReadOnlyDictionary<string, decimal> TotalsByCategory() =>
        expenses.GroupBy(expense => expense.Category, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.First().Category, group => group.Sum(expense => expense.Amount), StringComparer.OrdinalIgnoreCase);
}
