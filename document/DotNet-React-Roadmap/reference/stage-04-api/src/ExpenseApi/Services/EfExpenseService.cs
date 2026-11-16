using ExpenseApi.Data;
using ExpenseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Services;

public sealed class EfExpenseService(ExpenseDbContext db, ILogger<EfExpenseService> logger) : IExpenseService
{
    public async Task<PagedResult<ExpenseResponse>> GetExpensesAsync(int page, int pageSize, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        if (from.HasValue && to.HasValue && from > to) throw new ArgumentException("Ngày bắt đầu không được sau ngày kết thúc.");

        var query = db.Expenses.AsNoTracking().Where(x => (!from.HasValue || x.SpentOn >= from) && (!to.HasValue || x.SpentOn <= to));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.SpentOn).ThenByDescending(x => x.Id).Skip(checked((page - 1) * pageSize)).Take(pageSize)
            .Select(ToResponse).ToListAsync(cancellationToken);
        var pages = total == 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);
        return new PagedResult<ExpenseResponse>(items, total, page, pageSize, pages);
    }

    public Task<ExpenseResponse?> GetExpenseByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Expenses.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).SingleOrDefaultAsync(cancellationToken);

    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        if (year is < 1 or > 9999) throw new ArgumentOutOfRangeException(nameof(year));
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));
        var start = new DateOnly(year, month, 1);
        var monthly = db.Expenses.AsNoTracking().Where(x => x.SpentOn >= start);
        if (year != 9999 || month != 12)
        {
            var end = start.AddMonths(1);
            monthly = monthly.Where(x => x.SpentOn < end);
        }
        var grouped = await monthly
            .GroupBy(x => new { x.CategoryId, x.Category.Name })
            .Select(g => new { g.Key.CategoryId, CategoryName = g.Key.Name, Total = g.Sum(x => x.Amount), Count = g.Count() })
            .OrderBy(x => x.CategoryName).ToListAsync(cancellationToken);
        var rows = grouped.Select(x => new CategoryMonthlySummary(x.CategoryId, x.CategoryName, x.Total, x.Count)).ToList();
        return new MonthlySummaryResponse(year, month, rows.Sum(x => x.Total), rows.Sum(x => x.Count), rows);
    }

    public async Task<ExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateExpense(request.CategoryId, request.Amount, request.SpentOn, request.Note);
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"CategoryId {request.CategoryId} không tồn tại.");
        var expense = new Data.Expense { CategoryId = category.Id, Amount = request.Amount, SpentOn = request.SpentOn!.Value, Note = request.Note, CreatedAtUtc = DateTime.UtcNow };
        db.Expenses.Add(expense);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Expense created {ExpenseId} for category {CategoryId}", expense.Id, expense.CategoryId);
        return ToResponse.Compile().Invoke(expense);
    }

    public async Task<ExpenseResponse?> UpdateExpenseAsync(int id, UpdateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateExpense(request.CategoryId, request.Amount, request.SpentOn, request.Note);
        var expense = await db.Expenses.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (expense is null) return null;
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"CategoryId {request.CategoryId} không tồn tại.");
        expense.CategoryId = category.Id;
        expense.Category = category;
        expense.Amount = request.Amount;
        expense.SpentOn = request.SpentOn!.Value;
        expense.Note = request.Note;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Expense updated {ExpenseId} for category {CategoryId}", expense.Id, expense.CategoryId);
        return ToResponse.Compile().Invoke(expense);
    }

    public async Task<bool> DeleteExpenseAsync(int id, CancellationToken cancellationToken = default)
    {
        var expense = await db.Expenses.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (expense is null) return false;
        db.Expenses.Remove(expense);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new CategoryResponse(x.Id, x.Name)).ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse> AddCategoryAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = ValidateCategory(name);
        var category = new Category { Name = normalized };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryResponse(category.Id, category.Name);
    }

    private static readonly System.Linq.Expressions.Expression<Func<Data.Expense, ExpenseResponse>> ToResponse = x =>
        new(x.Id, x.CategoryId, x.Category.Name, x.Amount, x.SpentOn, x.Note);

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
    }

    private static void ValidateExpense(int categoryId, decimal amount, DateOnly? spentOn, string? note)
    {
        if (categoryId <= 0) throw new ArgumentException("CategoryId phải lớn hơn 0.", nameof(categoryId));
        if (amount < 0 || amount > 9_999_999_999_999_999.99m || amount != decimal.Round(amount, 2)) throw new ArgumentException("Amount phải từ 0 đến decimal(18,2), tối đa 2 chữ số thập phân.", nameof(amount));
        if (!spentOn.HasValue) throw new ArgumentException("SpentOn là bắt buộc.", nameof(spentOn));
        if (note is not null && note.Length > 500) throw new ArgumentException("Note tối đa 500 ký tự.", nameof(note));
    }

    private static string ValidateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100) throw new ArgumentException("Tên danh mục không hợp lệ.", nameof(name));
        return name.Trim();
    }
}
