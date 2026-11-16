using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

public sealed class MigrationIntegrationTests
{
    [Fact]
    public async Task Fresh_fixture_applies_ordered_migrations_and_persists_linked_receipt_atomically()
    {
        var database = $"RoadmapAuth11_{Guid.NewGuid():N}";
        Console.WriteLine($"AUTH_MIGRATION_FIXTURE_DATABASE={database}");
        Assert.Matches(@"\ARoadmapAuth11_[0-9a-f]{32}\z", database);

        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=True;TrustServerCertificate=True;";
        await RefuseExistingDatabaseAsync(database);

        var options = new DbContextOptionsBuilder<AuthDb>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new AuthDb(options);
        var migrator = context.Database.GetService<IMigrator>();
        const string baseline = "20261009001000_InitialStage11IdentityExpense";
        const string latest = "20261009002000_AddExpenseIdempotencyReceipts";

        await migrator.MigrateAsync(baseline);
        Assert.Equal(new[] { baseline }, (await context.Database.GetAppliedMigrationsAsync()).ToArray());
        Assert.Equal(new[]
        {
            "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins",
            "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens", "Expenses"
        }, await ReadTableNamesAsync(context));
        await AssertBaselineKeysAndForeignKeysAsync(context);
        var baselineExpenseColumns = await ReadColumnsAsync(context, "Expenses");
        AssertExpenseSchema(baselineExpenseColumns);
        Assert.True(await TableExistsAsync(context, "Expenses"));
        Assert.False(await TableExistsAsync(context, "ExpenseIdempotencyReceipts"));
        var legacy = new Expense { OwnerUserId = "legacy-owner", Amount = 12.34m, Note = "legacy expense" };
        context.Expenses.Add(legacy);
        await context.SaveChangesAsync();

        await migrator.MigrateAsync(latest);
        Assert.Equal(new[] { baseline, latest }, (await context.Database.GetAppliedMigrationsAsync()).ToArray());
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.Equal(new[]
        {
            "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins",
            "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens", "ExpenseIdempotencyReceipts", "Expenses"
        }, await ReadTableNamesAsync(context));
        AssertExpenseSchema(await ReadColumnsAsync(context, "Expenses"), baselineExpenseColumns);
        Assert.Empty(await context.ExpenseIdempotencyReceipts.AsNoTracking().ToListAsync());

        await AssertReceiptSchemaAsync(context);
        var persistedLegacy = await context.Expenses.AsNoTracking().SingleAsync(expense => expense.Id == legacy.Id);
        Assert.Equal(legacy.OwnerUserId, persistedLegacy.OwnerUserId);
        Assert.Equal(legacy.Amount, persistedLegacy.Amount);
        Assert.Equal(legacy.Note, persistedLegacy.Note);

        var newExpense = new Expense { OwnerUserId = "atomic-owner", Amount = 45.67m, Note = "atomic note" };
        context.Expenses.Add(newExpense);
        var temporaryId = context.Entry(newExpense).Property(expense => expense.Id).CurrentValue;
        var receipt = new ExpenseIdempotencyReceipt
        {
            OwnerUserId = newExpense.OwnerUserId,
            NormalizedKey = "atomic-key",
            RequestHash = new string('A', 64),
            ExpenseId = temporaryId,
            OriginalAmount = newExpense.Amount,
            OriginalNote = newExpense.Note
        };
        context.ExpenseIdempotencyReceipts.Add(receipt);
        await context.SaveChangesAsync();

        Assert.True(newExpense.Id > 0);
        Assert.Equal(newExpense.Id, receipt.ExpenseId);
        var persistedReceipt = await context.ExpenseIdempotencyReceipts.AsNoTracking().SingleAsync(item => item.Id == receipt.Id);
        Assert.Equal(newExpense.Id, persistedReceipt.ExpenseId);
        Assert.Equal(newExpense.OwnerUserId, persistedReceipt.OwnerUserId);
        Assert.Equal(newExpense.Amount, persistedReceipt.OriginalAmount);
        Assert.Equal(newExpense.Note, persistedReceipt.OriginalNote);
        Assert.Equal(new string('A', 64), persistedReceipt.RequestHash);
    }

    private static async Task RefuseExistingDatabaseAsync(string database)
    {
        await using var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name=@name";
        command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = database });
        var exists = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(0, exists);
    }

    private static async Task<string[]> ReadTableNamesAsync(AuthDb context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT name FROM sys.tables WHERE is_ms_shipped=0 AND name <> '__EFMigrationsHistory' ORDER BY name";
        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();
        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            names.Add(reader.GetString(0));
        return names.ToArray();
    }

    private static async Task<bool> TableExistsAsync(AuthDb context, string table)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped=0 AND name=@name";
        command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = table });
        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    private static async Task AssertReceiptSchemaAsync(AuthDb context)
    {
        var columns = await ReadColumnsAsync(context, "ExpenseIdempotencyReceipts");
        Assert.Equal(("int", 4, false, 10, 0, true, ""), columns["Id"]);
        Assert.Equal(("nvarchar", 900, false, 0, 0, false, "SQL_Latin1_General_CP1_CI_AS"), columns["OwnerUserId"]);
        Assert.Equal(("varchar", 128, false, 0, 0, false, "Latin1_General_100_BIN2"), columns["NormalizedKey"]);
        Assert.Equal(("char", 64, false, 0, 0, false, "SQL_Latin1_General_CP1_CI_AS"), columns["RequestHash"]);
        Assert.Equal(("int", 4, false, 10, 0, false, ""), columns["ExpenseId"]);
        Assert.Equal(("decimal", 9, false, 18, 2, false, ""), columns["OriginalAmount"]);
        Assert.Equal(("nvarchar", 1000, false, 0, 0, false, "SQL_Latin1_General_CP1_CI_AS"), columns["OriginalNote"]);

        var indexes = await ReadIndexesAsync(context);
        Assert.Contains(indexes, index => index.Name == "PK_ExpenseIdempotencyReceipts" && index.Unique && index.Clustered && index.Columns.SequenceEqual(new[] { "Id" }));
        Assert.Contains(indexes, index => index.Name == "UX_ExpenseIdempotencyReceipts_Owner_Key" && index.Unique && !index.Clustered && index.Columns.SequenceEqual(new[] { "OwnerUserId", "NormalizedKey" }));
        var foreignKey = await ReadForeignKeyAsync(context);
        Assert.Equal("Expenses", foreignKey.PrincipalTable);
        Assert.Equal("NO_ACTION", foreignKey.DeleteAction);
    }

    private static async Task AssertBaselineKeysAndForeignKeysAsync(AuthDb context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT t.name,i.name FROM sys.tables t JOIN sys.indexes i ON t.object_id=i.object_id WHERE t.is_ms_shipped=0 AND t.name <> '__EFMigrationsHistory' AND i.is_primary_key=1 ORDER BY t.name; SELECT OBJECT_NAME(f.parent_object_id),OBJECT_NAME(f.referenced_object_id),f.name,f.delete_referential_action_desc FROM sys.foreign_keys f ORDER BY OBJECT_NAME(f.parent_object_id),f.name";
        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();
        var primaryKeys = new List<(string Table, string Key)>();
        var foreignKeys = new List<(string Parent, string Principal, string Name, string DeleteAction)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            primaryKeys.Add((reader.GetString(0), reader.GetString(1)));
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
            foreignKeys.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));

        Assert.Equal(new[]
        {
            ("AspNetRoleClaims", "PK_AspNetRoleClaims"),
            ("AspNetRoles", "PK_AspNetRoles"),
            ("AspNetUserClaims", "PK_AspNetUserClaims"),
            ("AspNetUserLogins", "PK_AspNetUserLogins"),
            ("AspNetUserRoles", "PK_AspNetUserRoles"),
            ("AspNetUsers", "PK_AspNetUsers"),
            ("AspNetUserTokens", "PK_AspNetUserTokens"),
            ("Expenses", "PK_Expenses")
        }, primaryKeys);
        Assert.Equal(new[]
        {
            ("AspNetRoleClaims", "AspNetRoles", "FK_AspNetRoleClaims_AspNetRoles_RoleId", "CASCADE"),
            ("AspNetUserClaims", "AspNetUsers", "FK_AspNetUserClaims_AspNetUsers_UserId", "CASCADE"),
            ("AspNetUserLogins", "AspNetUsers", "FK_AspNetUserLogins_AspNetUsers_UserId", "CASCADE"),
            ("AspNetUserRoles", "AspNetRoles", "FK_AspNetUserRoles_AspNetRoles_RoleId", "CASCADE"),
            ("AspNetUserRoles", "AspNetUsers", "FK_AspNetUserRoles_AspNetUsers_UserId", "CASCADE"),
            ("AspNetUserTokens", "AspNetUsers", "FK_AspNetUserTokens_AspNetUsers_UserId", "CASCADE")
        }, foreignKeys);
    }

    private static void AssertExpenseSchema(Dictionary<string, (string Type, int MaxLength, bool Nullable, byte Precision, byte Scale, bool Identity, string Collation)> columns, Dictionary<string, (string Type, int MaxLength, bool Nullable, byte Precision, byte Scale, bool Identity, string Collation)>? expected = null)
    {
        Assert.Equal(("int", 4, false, 10, 0, true, ""), columns["Id"]);
        Assert.Equal(("nvarchar", 900, false, 0, 0, false, "SQL_Latin1_General_CP1_CI_AS"), columns["OwnerUserId"]);
        Assert.Equal(("decimal", 9, false, 18, 2, false, ""), columns["Amount"]);
        Assert.Equal(("nvarchar", -1, false, 0, 0, false, "SQL_Latin1_General_CP1_CI_AS"), columns["Note"]);
        if (expected is not null)
            Assert.Equal(expected, columns);
    }

    private static async Task<Dictionary<string, (string Type, int MaxLength, bool Nullable, byte Precision, byte Scale, bool Identity, string Collation)>> ReadColumnsAsync(AuthDb context, string table)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT c.name,t.name,c.max_length,c.is_nullable,c.precision,c.scale,COLUMNPROPERTY(c.object_id,c.name,'IsIdentity'),c.collation_name FROM sys.columns c JOIN sys.types t ON c.user_type_id=t.user_type_id WHERE c.object_id=OBJECT_ID(@table)";
        command.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = table });
        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();
        var result = new Dictionary<string, (string, int, bool, byte, byte, bool, string)>(StringComparer.Ordinal);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result[reader.GetString(0)] = (reader.GetString(1), reader.GetInt16(2), reader.GetBoolean(3), reader.GetByte(4), reader.GetByte(5), reader.GetInt32(6) == 1, reader.IsDBNull(7) ? "" : reader.GetString(7));
        return result;
    }

    private static async Task<List<(string Name, bool Unique, bool Clustered, string[] Columns)>> ReadIndexesAsync(AuthDb context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT i.name,i.is_unique,i.type_desc,ic.key_ordinal,c.name FROM sys.indexes i JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id WHERE i.object_id=OBJECT_ID(@table) AND ic.key_ordinal > 0 ORDER BY i.index_id,ic.key_ordinal";
        command.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = "ExpenseIdempotencyReceipts" });
        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();
        var rows = new List<(string Name, bool Unique, bool Clustered, int Ordinal, string Column)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            rows.Add((reader.GetString(0), reader.GetBoolean(1), reader.GetString(2) == "CLUSTERED", reader.GetByte(3), reader.GetString(4)));
        return rows.GroupBy(row => (row.Name, row.Unique, row.Clustered)).Select(group => (group.Key.Name, group.Key.Unique, group.Key.Clustered, group.OrderBy(row => row.Ordinal).Select(row => row.Column).ToArray())).ToList();
    }

    private static async Task<(string PrincipalTable, string DeleteAction)> ReadForeignKeyAsync(AuthDb context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT OBJECT_NAME(f.referenced_object_id),f.delete_referential_action_desc FROM sys.foreign_keys f WHERE f.parent_object_id=OBJECT_ID(@table)";
        command.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = "ExpenseIdempotencyReceipts" });
        if (command.Connection!.State != ConnectionState.Open)
            await command.Connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        return (reader.GetString(0), reader.GetString(1));
    }
}
