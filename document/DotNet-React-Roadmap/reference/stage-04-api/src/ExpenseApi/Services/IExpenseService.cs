namespace ExpenseApi.Services;

using ExpenseApi.Models;

public interface IExpenseService
{
    Task<PagedResult<ExpenseResponse>> GetExpensesAsync(
        int page,
        int pageSize,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<ExpenseResponse?> GetExpenseByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<MonthlySummaryResponse> GetMonthlySummaryAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default);

    Task<ExpenseResponse> CreateExpenseAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default);

    Task<ExpenseResponse?> UpdateExpenseAsync(
        int id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteExpenseAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<CategoryResponse> AddCategoryAsync(
        string name,
        CancellationToken cancellationToken = default);
}
