using System.Data;
using ExpenseSqlGuided;
using Microsoft.Data.SqlClient;

namespace ExpenseSqlGuided.Tests;

public sealed class SqlTests
{
    [Fact]
    public void Invalid_database_names_are_rejected_before_any_connection()
    {
        Assert.Throws<ArgumentException>(() => SqlSchema.MasterConnection("master"));
        Assert.Throws<ArgumentNullException>(() => SqlSchema.MasterConnection(null!));
        Assert.Throws<ArgumentException>(() => SqlSchema.MasterConnection("RoadmapSql735_00000000000000000000000000000000\n"));
        Assert.Throws<ArgumentException>(() => new SqlExpenseStore("RoadmapSql735_bad"));
    }

    [Fact]
    public async Task Domain_validation_rejects_rounding_overflow_and_length_before_provider()
    {
        SqlExpenseStore store = await NewStore();
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddCategoryAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => store.AddCategoryAsync(" "));
        await Assert.ThrowsAsync<ArgumentException>(() => store.AddCategoryAsync(new string('x', 101)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.AddExpenseAsync(1, -1m, new DateOnly(2026, 1, 1), null));
        await Assert.ThrowsAsync<ArgumentException>(() => store.AddExpenseAsync(1, 1.234m, new DateOnly(2026, 1, 1), null));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.AddExpenseAsync(1, 99_999_999_999_999_999.99m, new DateOnly(2026, 1, 1), null));
        await Assert.ThrowsAsync<ArgumentException>(() => store.AddExpenseAsync(1, 1m, new DateOnly(2026, 1, 1), new string('n', 501)));
        int categoryId = await store.AddCategoryAsync("Max");
        int maxId = await store.AddExpenseAsync(categoryId, 9_999_999_999_999_999.99m, new DateOnly(2026, 1, 1), new string('n', 500));
        SqlExpense max = (await store.PageAsync(1, 10)).Single(item => item.Id == maxId);
        Assert.Equal(9_999_999_999_999_999.99m, max.Amount); Assert.Equal(500, max.Note!.Length);
    }

    [Fact]
    public async Task Raw_schema_enforces_exact_null_negative_fk_unique_and_referenced_delete_errors()
    {
        SqlExpenseStore store = await NewStore();
        int categoryId = await store.AddCategoryAsync("O'Reilly");

        await AssertSqlNumber(store, "INSERT INTO Categories(Name) VALUES (NULL);", 515);
        await AssertSqlNumber(store, "INSERT INTO Expenses(CategoryId, Amount, SpentOn) VALUES (@category, NULL, @date);", 515, ("@category", categoryId), ("@date", new DateTime(2026, 1, 1)));
        await AssertSqlNumber(store, "INSERT INTO Expenses(CategoryId, Amount, SpentOn) VALUES (@category, 1, NULL);", 515, ("@category", categoryId));
        await AssertSqlError(store, "INSERT INTO Expenses(CategoryId, Amount, SpentOn) VALUES (@category, -1, @date);", 547, "CK_Expenses_Amount_NonNegative", ("@category", categoryId), ("@date", new DateTime(2026, 1, 1)));
        await AssertSqlError(store, "INSERT INTO Expenses(CategoryId, Amount, SpentOn) VALUES (999999, 1, @date);", 547, "FK_Expenses_Categories", ("@date", new DateTime(2026, 1, 1)));
        await AssertSqlError(store, "INSERT INTO Categories(Name) VALUES (@name);", 2627, "UQ_Categories_Name", ("@name", "o'reilly"));

        await store.AddExpenseAsync(categoryId, 0m, new DateOnly(2026, 1, 1), null);
        await AssertSqlError(store, "DELETE FROM Categories WHERE Id=@id;", 547, "FK_Expenses_Categories", ("@id", categoryId));
        Assert.Null((await store.PageAsync(1, 10)).Single().Note);
    }

    [Fact]
    public async Task Crud_is_parameterized_update_changes_values_delete_removes_and_pages_are_stable()
    {
        SqlExpenseStore store = await NewStore();
        int categoryId = await store.AddCategoryAsync("Quotes ' safe");
        await store.AddCategoryAsync("Unused");
        int first = await store.AddExpenseAsync(categoryId, 10.10m, new DateOnly(2026, 3, 2), "note ' one");
        int second = await store.AddExpenseAsync(categoryId, 20m, new DateOnly(2026, 3, 1), null);
        int third = await store.AddExpenseAsync(categoryId, 30m, new DateOnly(2026, 3, 3), "third");

        IReadOnlyList<SqlExpense> page1 = await store.PageAsync(1, 2);
        IReadOnlyList<SqlExpense> page2 = await store.PageAsync(2, 2);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.PageAsync(0, 2));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.PageAsync(int.MaxValue, int.MaxValue));
        Assert.Equal(new[] { first, second }, page1.Select(item => item.Id));
        Assert.Equal(new[] { third }, page2.Select(item => item.Id));
        Assert.Empty(page1.Select(item => item.Id).Intersect(page2.Select(item => item.Id)));

        string note500 = new string('n', 500);
        await Assert.ThrowsAsync<ArgumentException>(() => store.UpdateExpenseAsync(first, categoryId, 1.234m, new DateOnly(2026, 4, 4), null));
        await Assert.ThrowsAsync<SqlException>(() => store.UpdateExpenseAsync(first, 999999, 11.11m, new DateOnly(2026, 4, 4), note500));
        SqlExpense beforeFailedUpdate = (await store.PageAsync(1, 10)).Single(item => item.Id == first);
        Assert.Equal(10.10m, beforeFailedUpdate.Amount); Assert.Equal("note ' one", beforeFailedUpdate.Note);
        await store.UpdateExpenseAsync(first, categoryId, 11.11m, new DateOnly(2026, 4, 4), note500);
        SqlExpense changed = (await store.PageAsync(1, 10)).Single(item => item.Id == first);
        Assert.Equal(11.11m, changed.Amount); Assert.Equal(new DateOnly(2026, 4, 4), changed.SpentOn); Assert.Equal(note500, changed.Note);
        await store.DeleteExpenseAsync(second);
        Assert.DoesNotContain((await store.PageAsync(1, 10)).Select(item => item.Id), id => id == second);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => store.UpdateExpenseAsync(999999, categoryId, 1m, new DateOnly(2026, 1, 1), null));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => store.DeleteExpenseAsync(999999));
        Assert.Contains((await store.CategoriesWithoutExpensesAsync()), item => item.Name == "Unused");
    }

    [Fact]
    public async Task Monthly_totals_separate_month_and_year()
    {
        SqlExpenseStore store = await NewStore(); int categoryId = await store.AddCategoryAsync("Totals");
        await store.AddExpenseAsync(categoryId, 10m, new DateOnly(2026, 1, 1), null);
        await store.AddExpenseAsync(categoryId, 2.25m, new DateOnly(2026, 1, 31), null);
        await store.AddExpenseAsync(categoryId, 7m, new DateOnly(2026, 2, 1), null);
        await store.AddExpenseAsync(categoryId, 4m, new DateOnly(2027, 1, 1), null);
        IReadOnlyList<MonthlyTotal> totals = await store.MonthlyTotalsAsync();
        Assert.Equal(12.25m, totals.Single(item => item.Year == 2026 && item.Month == 1).Total);
        Assert.Equal(7m, totals.Single(item => item.Year == 2026 && item.Month == 2).Total);
        Assert.Equal(4m, totals.Single(item => item.Year == 2027 && item.Month == 1).Total);
    }

    [Fact]
    public async Task Transaction_is_visible_inside_then_rollback_restores_previous_count()
    {
        SqlExpenseStore store = await NewStore(); int categoryId = await store.AddCategoryAsync("Transaction");
        int before = (await store.PageAsync(1, 100)).Count;
        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync();
        await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO Expenses(CategoryId, Amount, SpentOn) VALUES (@category, @amount, @date); SELECT COUNT(*) FROM Expenses;";
        command.Parameters.Add("@category", SqlDbType.Int).Value = categoryId;
        command.Parameters.Add("@amount", SqlDbType.Decimal).Value = 9.99m;
        command.Parameters["@amount"].Precision = 18; command.Parameters["@amount"].Scale = 2;
        command.Parameters.Add("@date", SqlDbType.Date).Value = new DateTime(2026, 5, 1);
        Assert.Equal(before + 1, (int)(await command.ExecuteScalarAsync())!);
        await transaction.RollbackAsync();
        Assert.Equal(before, (await store.PageAsync(1, 100)).Count);
    }

    [Fact]
    public async Task Creating_existing_generated_database_fails_without_reuse()
    {
        string databaseName = SqlSchema.NewDatabaseName();
        await SqlSchema.CreateDatabaseAsync(databaseName);
        await Assert.ThrowsAsync<InvalidOperationException>(() => SqlSchema.CreateDatabaseAsync(databaseName));
    }

    private static async Task<SqlExpenseStore> NewStore()
    {
        string databaseName = SqlSchema.NewDatabaseName();
        await SqlSchema.CreateDatabaseAsync(databaseName);
        await SqlSchema.CreateSchemaAsync(databaseName);
        return new SqlExpenseStore(databaseName);
    }

    private static async Task AssertSqlNumber(SqlExpenseStore store, string sql, int expectedNumber, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach ((string name, object value) in parameters) command.Parameters.AddWithValue(name, value);
        SqlException error = await Assert.ThrowsAsync<SqlException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(expectedNumber, error.Number);
    }

    private static async Task AssertSqlError(SqlExpenseStore store, string sql, int expectedNumber, string constraintName, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqlConnection(store.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach ((string name, object value) in parameters) command.Parameters.AddWithValue(name, value);
        SqlException error = await Assert.ThrowsAsync<SqlException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(expectedNumber, error.Number);
        Assert.Contains(constraintName, error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
