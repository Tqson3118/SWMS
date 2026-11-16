namespace ExpenseGuided;

public sealed class DatedExpenseService
{
    private readonly List<DatedExpense> expenses = [];

    public IReadOnlyList<DatedExpense> List() => expenses.AsReadOnly();

    public void Add(DatedExpense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);
        if (expenses.Any(item => item.Id == expense.Id)) throw new InvalidOperationException("Id đã tồn tại.");
        expenses.Add(expense);
    }

    public bool Update(int id, string category, decimal amount, DateOnly spentOn)
    {
        int index = expenses.FindIndex(item => item.Id == id);
        if (index < 0) return false;
        var replacement = new DatedExpense(id, category, amount, spentOn);
        expenses[index] = replacement;
        return true;
    }

    public bool Remove(int id)
    {
        int index = expenses.FindIndex(item => item.Id == id);
        if (index < 0) return false;
        expenses.RemoveAt(index);
        return true;
    }

    public decimal TotalForMonth(int year, int month) =>
        expenses.Where(item => item.SpentOn.Year == year && item.SpentOn.Month == month).Sum(item => item.Amount);
}
