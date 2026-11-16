namespace ExpenseApi.Services;

using System.Data;
using ExpenseApi.Models;
using ExpenseSqlGuided;
using Microsoft.Data.SqlClient;

public sealed class SqlExpenseService(SqlExpenseStore store) : IExpenseService
{
    public SqlExpenseStore Store => store;

    public async Task<PagedResult<ExpenseResponse>> GetExpensesAsync(
        int page,
        int pageSize,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page phải lớn hơn hoặc bằng 1.");
        if (pageSize < 1 || pageSize > 100) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize phải từ 1 đến 100.");
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentException("Ngày bắt đầu (from) không được sau ngày kết thúc (to).");

        int skip = checked((page - 1) * pageSize);

        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        long totalCount;
        await using (var countCommand = connection.CreateCommand())
        {
            countCommand.CommandText = """
                SELECT COUNT_BIG(*)
                FROM Expenses e
                WHERE (@from IS NULL OR e.SpentOn >= @from)
                  AND (@to IS NULL OR e.SpentOn <= @to);
                """;
            AddDateFilterParameters(countCommand, from, to);
            totalCount = (long)(await countCommand.ExecuteScalarAsync(cancellationToken))!;
        }

        var items = new List<ExpenseResponse>();
        await using (var queryCommand = connection.CreateCommand())
        {
            queryCommand.CommandText = """
                SELECT e.Id, e.CategoryId, c.Name, e.Amount, e.SpentOn, e.Note
                FROM Expenses e
                JOIN Categories c ON c.Id = e.CategoryId
                WHERE (@from IS NULL OR e.SpentOn >= @from)
                  AND (@to IS NULL OR e.SpentOn <= @to)
                ORDER BY e.Id
                OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY;
                """;
            AddDateFilterParameters(queryCommand, from, to);
            queryCommand.Parameters.Add("@skip", SqlDbType.Int).Value = skip;
            queryCommand.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize;

            await using var reader = await queryCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new ExpenseResponse(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetDecimal(3),
                    DateOnly.FromDateTime(reader.GetDateTime(4)),
                    reader.IsDBNull(5) ? null : reader.GetString(5)));
            }
        }

        int totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResult<ExpenseResponse>(items, (int)totalCount, page, pageSize, totalPages);
    }

    public async Task<ExpenseResponse?> GetExpenseByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT e.Id, e.CategoryId, c.Name, e.Amount, e.SpentOn, e.Note
            FROM Expenses e
            JOIN Categories c ON c.Id = e.CategoryId
            WHERE e.Id = @id;
            """;
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new ExpenseResponse(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDecimal(3),
                DateOnly.FromDateTime(reader.GetDateTime(4)),
                reader.IsDBNull(5) ? null : reader.GetString(5));
        }

        return null;
    }

    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (year < 1900 || year > 2100) throw new ArgumentOutOfRangeException(nameof(year), "Year phải từ 1900 đến 2100.");
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month), "Month phải từ 1 đến 12.");

        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.Id, c.Name, SUM(e.Amount) AS Total, COUNT_BIG(e.Id) AS [Count]
            FROM Expenses e
            JOIN Categories c ON c.Id = e.CategoryId
            WHERE YEAR(e.SpentOn) = @year AND MONTH(e.SpentOn) = @month
            GROUP BY c.Id, c.Name
            ORDER BY c.Name;
            """;
        command.Parameters.Add("@year", SqlDbType.Int).Value = year;
        command.Parameters.Add("@month", SqlDbType.Int).Value = month;

        var categories = new List<CategoryMonthlySummary>();
        decimal grandTotal = 0m;
        int totalCount = 0;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var total = reader.GetDecimal(2);
            var count = (int)reader.GetInt64(3);
            categories.Add(new CategoryMonthlySummary(
                reader.GetInt32(0),
                reader.GetString(1),
                total,
                count));
            grandTotal += total;
            totalCount += count;
        }

        return new MonthlySummaryResponse(year, month, grandTotal, totalCount, categories);
    }

    public async Task<ExpenseResponse> CreateExpenseAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        string categoryName;
        await using (var connection = new SqlConnection(store.ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var categoryCheck = connection.CreateCommand();
            categoryCheck.CommandText = "SELECT Name FROM Categories WHERE Id = @categoryId;";
            categoryCheck.Parameters.Add("@categoryId", SqlDbType.Int).Value = request.CategoryId;
            var result = await categoryCheck.ExecuteScalarAsync(cancellationToken);
            if (result is null or DBNull)
            {
                throw new KeyNotFoundException($"CategoryId {request.CategoryId} không tồn tại.");
            }
            categoryName = (string)result;
        }

        int id = await store.AddExpenseAsync(request.CategoryId, request.Amount, request.SpentOn!.Value, request.Note);
        return new ExpenseResponse(id, request.CategoryId, categoryName, request.Amount, request.SpentOn!.Value, request.Note);
    }

    public async Task<ExpenseResponse?> UpdateExpenseAsync(
        int id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.SpentOn.HasValue) throw new ArgumentException("SpentOn là bắt buộc.", nameof(request));
        await store.UpdateExpenseAsync(id, request.CategoryId, request.Amount, request.SpentOn.Value, request.Note);
        return await GetExpenseByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteExpenseAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await store.DeleteExpenseAsync(id);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Categories ORDER BY Name;";

        var list = new List<CategoryResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new CategoryResponse(reader.GetInt32(0), reader.GetString(1)));
        }
        return list;
    }

    public async Task<CategoryResponse> AddCategoryAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        int id = await store.AddCategoryAsync(name);
        return new CategoryResponse(id, SqlRules.Category(name));
    }

    private static void AddDateFilterParameters(SqlCommand command, DateOnly? from, DateOnly? to)
    {
        var fromParam = command.Parameters.Add("@from", SqlDbType.Date);
        fromParam.Value = from.HasValue ? from.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;

        var toParam = command.Parameters.Add("@to", SqlDbType.Date);
        toParam.Value = to.HasValue ? to.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
    }
}
