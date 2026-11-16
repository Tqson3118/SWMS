using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public sealed class DesignTimeFactoryTests
{
    [Fact]
    public void Factory_creates_unopened_context_with_fictional_strict_localdb_name()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        var connection = context.Database.GetDbConnection();

        Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.Contains("RoadmapAuth11_", connection.ConnectionString, StringComparison.Ordinal);
        Assert.Contains("(localdb)\\MSSQLLocalDB", connection.ConnectionString, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", connection.ConnectionString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Model_contains_complete_existing_identity_and_expense_table_set()
    {
        using var context = CreateContext();
        var tables = context.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(name => name is not null)
            .Distinct()
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(new[]
        {
            "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins",
            "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens", "ExpenseIdempotencyReceipts", "Expenses"
        }, tables);
    }

    [Fact]
    public void Identity_relational_metadata_has_expected_properties_keys_and_indexes()
    {
        using var context = CreateContext();
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["AspNetRoleClaims"] = new[] { "ClaimType", "ClaimValue", "Id", "RoleId" },
            ["AspNetRoles"] = new[] { "ConcurrencyStamp", "Id", "Name", "NormalizedName" },
            ["AspNetUserClaims"] = new[] { "ClaimType", "ClaimValue", "Id", "UserId" },
            ["AspNetUserLogins"] = new[] { "LoginProvider", "ProviderDisplayName", "ProviderKey", "UserId" },
            ["AspNetUserRoles"] = new[] { "RoleId", "UserId" },
            ["AspNetUserTokens"] = new[] { "LoginProvider", "Name", "UserId", "Value" },
            ["AspNetUsers"] = new[]
            {
                "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "Id", "LockoutEnabled",
                "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber",
                "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName"
            }
        };
        var expectedKeys = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["AspNetRoleClaims"] = new[] { "Id" },
            ["AspNetRoles"] = new[] { "Id" },
            ["AspNetUserClaims"] = new[] { "Id" },
            ["AspNetUserLogins"] = new[] { "LoginProvider", "ProviderKey" },
            ["AspNetUserRoles"] = new[] { "UserId", "RoleId" },
            ["AspNetUserTokens"] = new[] { "UserId", "LoginProvider", "Name" },
            ["AspNetUsers"] = new[] { "Id" }
        };

        var identityTables = context.Model.GetEntityTypes()
            .Where(entity => entity.GetTableName() is not "Expenses" and not "ExpenseIdempotencyReceipts")
            .Select(entity => entity.GetTableName()!)
            .OrderBy(name => name)
            .ToArray();
        Assert.Equal(expected.Keys.OrderBy(name => name), identityTables);

        foreach (var entity in context.Model.GetEntityTypes().Where(entity => entity.GetTableName() is not "Expenses" and not "ExpenseIdempotencyReceipts"))
        {
            var table = entity.GetTableName()!;
            Assert.Equal(expected[table], entity.GetProperties().Select(property => property.Name).OrderBy(name => name));
            Assert.Contains(entity.GetKeys(), key => key.Properties.Select(property => property.Name).SequenceEqual(expectedKeys[table]));
        }

        var roles = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "AspNetRoles");
        Assert.Contains(roles.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "NormalizedName" }));
        var users = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "AspNetUsers");
        Assert.Contains(users.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "NormalizedUserName" }));
        Assert.Contains(users.GetIndexes(), index => !index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "NormalizedEmail" }));
    }

    [Fact]
    public void Expense_relational_metadata_has_expected_columns_precision_key_and_owner_index()
    {
        using var context = CreateContext();
        var expense = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "Expenses");

        Assert.Equal(new[] { "Amount", "Id", "Note", "OwnerUserId" }, expense.GetProperties().Select(property => property.Name).OrderBy(name => name));
        Assert.Equal(typeof(int), expense.FindProperty("Id")!.ClrType);
        Assert.Equal(ValueGenerated.OnAdd, expense.FindProperty("Id")!.ValueGenerated);
        Assert.Equal(typeof(decimal), expense.FindProperty("Amount")!.ClrType);
        Assert.Equal(18, expense.FindProperty("Amount")!.GetPrecision());
        Assert.Equal(2, expense.FindProperty("Amount")!.GetScale());
        Assert.False(expense.FindProperty("OwnerUserId")!.IsNullable);
        Assert.False(expense.FindProperty("Note")!.IsNullable);
        Assert.Contains(expense.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "OwnerUserId", "Id" }));
    }

    [Fact]
    public void Factory_model_inspection_does_not_require_environment_or_top_level_startup()
    {
        using var context = CreateContext();
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        Assert.Equal("RoadmapAuth11_0123456789abcdef0123456789abcdef", new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString).InitialCatalog);
    }

    private static AuthDb CreateContext() => new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
}
