using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ExpenseSqlGuided;

public sealed record SqlExpense(int Id, int CategoryId, string CategoryName, decimal Amount, DateOnly SpentOn, string? Note);
public sealed record MonthlyTotal(int Year, int Month, decimal Total);

public static class SqlSchema
{
    private static readonly Regex LearningDatabasePattern = new(@"\ARoadmapSql735_[0-9a-f]{32}\z", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string NewDatabaseName() => $"RoadmapSql735_{Guid.NewGuid():N}";

    public static void ValidateLearningDatabaseName(string databaseName)
    {
        ArgumentNullException.ThrowIfNull(databaseName);
        if (!LearningDatabasePattern.IsMatch(databaseName))
            throw new ArgumentException("Chỉ được dùng tên database học RoadmapSql735_<32 hex>.", nameof(databaseName));
    }

    public static string MasterConnection(string databaseName)
    {
        ValidateLearningDatabaseName(databaseName);
        Console.WriteLine($"Database trước khi tạo: {databaseName}");
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = "master",
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ApplicationName = "RoadmapSql735"
        };
        return builder.ConnectionString;
    }

    public static string DatabaseConnection(string databaseName)
    {
        ValidateLearningDatabaseName(databaseName);
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ApplicationName = "RoadmapSql735"
        };
        return builder.ConnectionString;
    }

    public static async Task CreateDatabaseAsync(string databaseName)
    {
        ValidateLearningDatabaseName(databaseName);
        string? logPath = Environment.GetEnvironmentVariable("DN45_SQL752_DB_LOG") ?? Environment.GetEnvironmentVariable("DN45_SQL735_DB_LOG");
        if (!string.IsNullOrWhiteSpace(logPath)) File.AppendAllText(logPath, databaseName + Environment.NewLine);
        await using var connection = new SqlConnection(MasterConnection(databaseName));
        await connection.OpenAsync();
        await using (var exists = connection.CreateCommand())
        {
            exists.CommandText = "SELECT COUNT_BIG(*) FROM sys.databases WHERE name = @name;";
            exists.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = databaseName;
            if ((long)(await exists.ExecuteScalarAsync())! != 0)
                throw new InvalidOperationException($"Database {databaseName} đã tồn tại; không tái sử dụng.");
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync();
        await Task.Delay(300);
    }

    public static async Task CreateSchemaAsync(string databaseName)
    {
        await using var connection = new SqlConnection(DatabaseConnection(databaseName));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Categories (
                Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
                Name NVARCHAR(100) COLLATE Latin1_General_100_CI_AS NOT NULL CONSTRAINT UQ_Categories_Name UNIQUE
            );
            CREATE TABLE Expenses (
                Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY,
                CategoryId INT NOT NULL CONSTRAINT FK_Expenses_Categories REFERENCES Categories(Id),
                Amount DECIMAL(18,2) NOT NULL CONSTRAINT CK_Expenses_Amount_NonNegative CHECK (Amount >= 0),
                SpentOn DATE NOT NULL,
                Note NVARCHAR(500) NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }
}

public static class SqlRules
{
    public static string Category(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        string value = name.Trim();
        if (value.Length == 0) throw new ArgumentException("Tên category không được rỗng.", nameof(name));
        if (value.Length > 100) throw new ArgumentException("Tên category tối đa 100 ký tự.", nameof(name));
        return value;
    }

    public static string? Note(string? note)
    {
        if (note is not null && note.Length > 500) throw new ArgumentException("Note tối đa 500 ký tự.", nameof(note));
        return note;
    }

    public static decimal Amount(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount không được âm.");
        if (amount != decimal.Round(amount, 2)) throw new ArgumentException("Amount tối đa 2 chữ số thập phân.", nameof(amount));
        if (amount > 9_999_999_999_999_999.99m) throw new ArgumentOutOfRangeException(nameof(amount), "Amount vượt decimal(18,2).");
        return amount;
    }
}

public sealed class SqlExpenseStore
{
    public SqlExpenseStore(string databaseName)
    {
        SqlSchema.ValidateLearningDatabaseName(databaseName);
        DatabaseName = databaseName;
        ConnectionString = SqlSchema.DatabaseConnection(databaseName);
    }

    public string DatabaseName { get; }
    public string ConnectionString { get; }

    public async Task<int> AddCategoryAsync(string name)
    {
        string normalizedName = SqlRules.Category(name);
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Categories(Name) OUTPUT INSERTED.Id VALUES (@name);";
        command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = normalizedName;
        return (int)(await command.ExecuteScalarAsync())!;
    }

    public async Task<int> AddExpenseAsync(int categoryId, decimal amount, DateOnly spentOn, string? note)
    {
        decimal validAmount = SqlRules.Amount(amount);
        string? validNote = SqlRules.Note(note);
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Expenses(CategoryId, Amount, SpentOn, Note) OUTPUT INSERTED.Id VALUES (@categoryId, @amount, @spentOn, @note);";
        AddExpenseParameters(command, categoryId, validAmount, spentOn, validNote);
        return (int)(await command.ExecuteScalarAsync())!;
    }

    public async Task UpdateExpenseAsync(int id, int categoryId, decimal amount, DateOnly spentOn, string? note)
    {
        decimal validAmount = SqlRules.Amount(amount);
        string? validNote = SqlRules.Note(note);
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Expenses SET CategoryId=@categoryId, Amount=@amount, SpentOn=@spentOn, Note=@note WHERE Id=@id;";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        AddExpenseParameters(command, categoryId, validAmount, spentOn, validNote);
        if (await command.ExecuteNonQueryAsync() == 0) throw new KeyNotFoundException($"Expense {id} không tồn tại.");
    }

    public async Task DeleteExpenseAsync(int id)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Expenses WHERE Id=@id;";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        if (await command.ExecuteNonQueryAsync() == 0) throw new KeyNotFoundException($"Expense {id} không tồn tại.");
    }

    public async Task<IReadOnlyList<SqlExpense>> PageAsync(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1) throw new ArgumentOutOfRangeException();
        int skip;
        try { skip = checked((page - 1) * pageSize); }
        catch (OverflowException) { throw new ArgumentOutOfRangeException(nameof(page), "Page offset vượt giới hạn."); }

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT e.Id, e.CategoryId, c.Name, e.Amount, e.SpentOn, e.Note
            FROM Expenses e JOIN Categories c ON c.Id=e.CategoryId
            ORDER BY e.Id OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY;
            """;
        command.Parameters.Add("@skip", SqlDbType.Int).Value = skip;
        command.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize;
        var result = new List<SqlExpense>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new SqlExpense(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDecimal(3),
                DateOnly.FromDateTime(reader.GetDateTime(4)), reader.IsDBNull(5) ? null : reader.GetString(5)));
        }
        return result;
    }

    public async Task<IReadOnlyList<MonthlyTotal>> MonthlyTotalsAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT YEAR(SpentOn), MONTH(SpentOn), SUM(Amount) FROM Expenses GROUP BY YEAR(SpentOn), MONTH(SpentOn) ORDER BY YEAR(SpentOn), MONTH(SpentOn);";
        var result = new List<MonthlyTotal>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new MonthlyTotal(reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2)));
        return result;
    }

    public async Task<IReadOnlyList<(int Id, string Name)>> CategoriesWithoutExpensesAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT c.Id, c.Name FROM Categories c LEFT JOIN Expenses e ON e.CategoryId=c.Id WHERE e.Id IS NULL ORDER BY c.Id;";
        var result = new List<(int, string)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add((reader.GetInt32(0), reader.GetString(1)));
        return result;
    }

    private static void AddExpenseParameters(SqlCommand command, int categoryId, decimal amount, DateOnly spentOn, string? note)
    {
        command.Parameters.Add("@categoryId", SqlDbType.Int).Value = categoryId;
        var amountParameter = command.Parameters.Add("@amount", SqlDbType.Decimal);
        amountParameter.Precision = 18; amountParameter.Scale = 2; amountParameter.Value = amount;
        command.Parameters.Add("@spentOn", SqlDbType.Date).Value = spentOn.ToDateTime(TimeOnly.MinValue);
        command.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value = (object?)note ?? DBNull.Value;
    }
}
