using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Reflection;
using Xunit;

public sealed class MigrationSchemaTests
{
    [Fact]
    public void Baseline_migration_has_the_exact_frozen_id()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        Assert.Contains("20261009001000_InitialStage11IdentityExpense", context.Database.GetMigrations());
    }

    [Fact]
    public void Latest_model_has_the_complete_identity_expense_and_receipt_table_set()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());

        Assert.Equal(new[]
        {
            "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins",
            "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens", "ExpenseIdempotencyReceipts", "Expenses"
        }, context.Model.GetEntityTypes().Select(entity => entity.GetTableName()).Where(name => name is not null).Distinct().OrderBy(name => name));
    }

    [Fact]
    public void Baseline_migration_preserves_the_frozen_eight_table_set()
    {
        var migration = new Stage11Auth.Migrations.InitialStage11IdentityExpense();
        var up = typeof(Migration).GetMethod("Up", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        up.Invoke(migration, new object[] { builder });

        Assert.Equal(new[]
        {
            "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins",
            "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens", "Expenses"
        }, builder.Operations.OfType<CreateTableOperation>().Select(operation => operation.Name).OrderBy(name => name));
    }

    [Fact]
    public void Migration_ids_are_ordered_baseline_then_receipt()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());

        Assert.Equal(new[]
        {
            "20261009001000_InitialStage11IdentityExpense",
            "20261009002000_AddExpenseIdempotencyReceipts"
        }, context.Database.GetMigrations());
    }

    [Fact]
    public void Baseline_model_preserves_frozen_keys_indexes_foreign_keys_and_money_shape()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        var expense = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "Expenses");

        Assert.Equal(typeof(decimal), expense.FindProperty("Amount")!.ClrType);
        Assert.Equal(18, expense.FindProperty("Amount")!.GetPrecision());
        Assert.Equal(2, expense.FindProperty("Amount")!.GetScale());
        Assert.False(expense.FindProperty("OwnerUserId")!.IsNullable);
        Assert.False(expense.FindProperty("Note")!.IsNullable);
        Assert.Contains(expense.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "OwnerUserId", "Id" }));

        var users = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "AspNetUsers");
        Assert.Contains(users.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "NormalizedUserName" }));
        Assert.Contains(users.GetIndexes(), index => !index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[] { "NormalizedEmail" }));

        var userClaims = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "AspNetUserClaims");
        Assert.Contains(userClaims.GetForeignKeys(), foreignKey => foreignKey.PrincipalEntityType.GetTableName() == "AspNetUsers" && foreignKey.Properties.Select(property => property.Name).SequenceEqual(new[] { "UserId" }));
        var roleClaims = context.Model.GetEntityTypes().Single(entity => entity.GetTableName() == "AspNetRoleClaims");
        Assert.Contains(roleClaims.GetForeignKeys(), foreignKey => foreignKey.PrincipalEntityType.GetTableName() == "AspNetRoles" && foreignKey.Properties.Select(property => property.Name).SequenceEqual(new[] { "RoleId" }));
    }

    [Fact]
    public void Baseline_down_refuses_destructive_rollback()
    {
        var migration = new Stage11Auth.Migrations.InitialStage11IdentityExpense();
        var down = typeof(Migration).GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var error = Assert.Throws<TargetInvocationException>(() => down.Invoke(migration, new object[] { new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer") }));
        Assert.IsType<NotSupportedException>(error.InnerException);
    }

    [Fact]
    public void Receipt_down_refuses_destructive_rollback()
    {
        var migration = new Stage11Auth.Migrations.AddExpenseIdempotencyReceipts();
        var down = typeof(Migration).GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var error = Assert.Throws<TargetInvocationException>(() => down.Invoke(migration, new object[] { new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer") }));
        Assert.IsType<NotSupportedException>(error.InnerException);
    }

    [Fact]
    public void Latest_target_and_snapshot_match_the_complete_finalized_relational_model()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        var migrations = context.GetService<IMigrationsAssembly>();
        var latestType = migrations.Migrations.Single(pair => pair.Key == "20261009002000_AddExpenseIdempotencyReceipts").Value;
        var latest = migrations.CreateMigration(latestType, context.Database.ProviderName!);
        var initializer = context.GetService<IModelRuntimeInitializer>();
        var target = initializer.Initialize(latest.TargetModel, designTime: true);
        var snapshot = initializer.Initialize(migrations.ModelSnapshot!.Model, designTime: true);
        var current = context.GetService<IDesignTimeModel>().Model;
        var differ = context.GetService<IMigrationsModelDiffer>();

        Assert.False(differ.HasDifferences(current.GetRelationalModel(), target.GetRelationalModel()));
        Assert.False(differ.HasDifferences(current.GetRelationalModel(), snapshot.GetRelationalModel()));
        Assert.False(differ.HasDifferences(target.GetRelationalModel(), snapshot.GetRelationalModel()));
        Assert.Equal(RelationalFingerprint(current), RelationalFingerprint(target));
        Assert.Equal(RelationalFingerprint(current), RelationalFingerprint(snapshot));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public void Baseline_to_receipt_delta_contains_only_receipt_table_and_unique_index()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        var migrations = context.GetService<IMigrationsAssembly>();
        var baselineType = migrations.Migrations.Single(pair => pair.Key == "20261009001000_InitialStage11IdentityExpense").Value;
        var latestType = migrations.Migrations.Single(pair => pair.Key == "20261009002000_AddExpenseIdempotencyReceipts").Value;
        var baseline = migrations.CreateMigration(baselineType, context.Database.ProviderName!);
        var latest = migrations.CreateMigration(latestType, context.Database.ProviderName!);
        var initializer = context.GetService<IModelRuntimeInitializer>();
        var baselineTarget = initializer.Initialize(baseline.TargetModel, designTime: true);
        var latestTarget = initializer.Initialize(latest.TargetModel, designTime: true);
        var operations = context.GetService<IMigrationsModelDiffer>().GetDifferences(
            baselineTarget.GetRelationalModel(), latestTarget.GetRelationalModel());

        Assert.Single(operations.OfType<CreateTableOperation>(), operation => operation.Name == "ExpenseIdempotencyReceipts");
        Assert.Single(operations.OfType<CreateIndexOperation>(), operation => operation.Name == "UX_ExpenseIdempotencyReceipts_Owner_Key");
        Assert.DoesNotContain(operations, operation => operation is DropTableOperation or AlterTableOperation or DropColumnOperation or AlterColumnOperation);
        var receipt = Assert.Single(operations.OfType<CreateTableOperation>(), operation => operation.Name == "ExpenseIdempotencyReceipts");
        var foreignKey = Assert.Single(receipt.ForeignKeys);
        Assert.Equal("Expenses", foreignKey.PrincipalTable);
        Assert.Equal(ReferentialAction.NoAction, foreignKey.OnDelete);
    }

    [Fact]
    public void Relational_model_differ_rejects_a_deliberate_in_memory_schema_drift()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        var driftBuilder = new ModelBuilder();
        var driftEntity = driftBuilder.Entity("DeliberateDrift");
        driftEntity.Property<int>("Id");
        driftEntity.HasKey("Id");

        var initializer = context.GetService<IModelRuntimeInitializer>();
        var drift = initializer.Initialize(driftBuilder.Model.FinalizeModel(), designTime: true);
        var differ = context.GetService<IMigrationsModelDiffer>();
        var current = context.GetService<IDesignTimeModel>().Model;
        Assert.True(differ.HasDifferences(current.GetRelationalModel(), drift.GetRelationalModel()));
        Assert.NotEqual(RelationalFingerprint(current), RelationalFingerprint(drift));
    }

    private static string RelationalFingerprint(IModel model)
    {
        var tables = model.GetEntityTypes()
            .Select(entity =>
            {
                var table = entity.GetTableName();
                var schema = entity.GetSchema();
                if (table is null)
                    return $"ENTITY|{entity.Name}|UNMAPPED|{string.Join(",", entity.GetProperties().Select(property => property.Name).Order())}";
                var store = StoreObjectIdentifier.Table(table, schema);
                var properties = entity.GetProperties().OrderBy(property => property.Name).Select(property =>
                    string.Join("|", property.Name, property.ClrType.FullName, property.GetColumnName(store), property.GetColumnType(store),
                        property.IsNullable, property.GetMaxLength(), property.GetPrecision(), property.GetScale(),
                        property.GetValueConverter()?.ProviderClrType?.FullName));
                var keys = entity.GetKeys().Select(key => string.Join(",", key.Properties.Select(property => property.Name)));
                var indexes = entity.GetIndexes().Select(index => string.Join("|", index.Name, index.IsUnique, index.GetFilter(), string.Join(",", index.Properties.Select(property => property.Name))));
                var foreignKeys = entity.GetForeignKeys().Select(foreignKey => string.Join("|", foreignKey.DeleteBehavior,
                    foreignKey.PrincipalEntityType.GetTableName(), string.Join(",", foreignKey.Properties.Select(property => property.Name)),
                    string.Join(",", foreignKey.PrincipalKey.Properties.Select(property => property.Name))));
                return string.Join("\n", new[] { $"TABLE|{schema}|{table}" }
                    .Concat(properties.Order())
                    .Concat(keys.Order())
                    .Concat(indexes.Order())
                    .Concat(foreignKeys.Order()));
            });
        var annotationValues = model.GetAnnotations()
            .Where(annotation => annotation.Name.StartsWith("Relational:", StringComparison.Ordinal) || annotation.Name.StartsWith("SqlServer:", StringComparison.Ordinal))
            .ToDictionary(annotation => annotation.Name, annotation => annotation.Value);
        if (annotationValues.TryGetValue("SqlServer:ValueGenerationStrategy", out var strategy) && string.Equals(strategy?.ToString(), "IdentityColumn", StringComparison.Ordinal))
        {
            annotationValues.TryAdd("SqlServer:IdentitySeed", 1L);
            annotationValues.TryAdd("SqlServer:IdentityIncrement", 1L);
        }
        var annotations = annotationValues.OrderBy(pair => pair.Key).Select(pair => $"ANNOTATION|{pair.Key}|{pair.Value}");
        return string.Join("\n", tables.Order().Concat(annotations));
    }
}
