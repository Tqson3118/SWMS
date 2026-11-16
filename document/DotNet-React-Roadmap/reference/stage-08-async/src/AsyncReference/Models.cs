namespace AsyncReference;

public readonly record struct ExpenseKey(int CategoryId, DateOnly SpentOn);

public sealed class ExpenseEntity
{
    public ExpenseEntity(int categoryId, decimal amount) => (CategoryId, Amount) = (categoryId, amount);
    public int CategoryId { get; }
    public decimal Amount { get; }
}

public static class ExpenseQueries
{
    public static IEnumerable<decimal> PositiveAmounts(IEnumerable<ExpenseEntity> source) =>
        source.Where(expense => expense.Amount > 0).Select(expense => expense.Amount);

    public static IEnumerable<TResult> Map<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> map)
    {
        foreach (var item in source) yield return map(item);
    }
}

public static class InputParsing
{
    public static bool TryParseDate(string? input, out DateOnly date) =>
        DateOnly.TryParseExact(input, "yyyy-MM-dd", out date);
}
