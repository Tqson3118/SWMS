using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public sealed class IdempotencyModelTests
{
    [Fact]
    public void Receipt_metadata_matches_the_frozen_contract()
    {
        using var context = new AuthDesignTimeFactory().CreateDbContext(Array.Empty<string>());
        var receipt = context.GetService<IDesignTimeModel>().Model.GetEntityTypes().Single(entity => entity.GetTableName() == "ExpenseIdempotencyReceipts");

        Assert.Equal(new[] { "ExpenseId", "Id", "NormalizedKey", "OriginalAmount", "OriginalNote", "OwnerUserId", "RequestHash" }, receipt.GetProperties().Select(property => property.Name).OrderBy(name => name));
        Assert.Equal(typeof(int), receipt.FindProperty("Id")!.ClrType);
        Assert.Equal(ValueGenerated.OnAdd, receipt.FindProperty("Id")!.ValueGenerated);
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, receipt.FindProperty("Id")!.GetValueGenerationStrategy());
        Assert.Equal(typeof(string), receipt.FindProperty("OwnerUserId")!.ClrType);
        Assert.Equal(typeof(string), receipt.FindProperty("NormalizedKey")!.ClrType);
        Assert.Equal(typeof(string), receipt.FindProperty("RequestHash")!.ClrType);
        Assert.Equal(typeof(int), receipt.FindProperty("ExpenseId")!.ClrType);
        Assert.Equal(typeof(decimal), receipt.FindProperty("OriginalAmount")!.ClrType);
        Assert.Equal(typeof(string), receipt.FindProperty("OriginalNote")!.ClrType);

        foreach (var property in receipt.GetProperties())
            Assert.False(property.IsNullable);
        Assert.Equal(450, receipt.FindProperty("OwnerUserId")!.GetMaxLength());
        Assert.Equal(128, receipt.FindProperty("NormalizedKey")!.GetMaxLength());
        Assert.Equal("nvarchar(450)", receipt.FindProperty("OwnerUserId")!.GetColumnType());
        Assert.False(receipt.FindProperty("NormalizedKey")!.IsUnicode());
        Assert.Equal("varchar(128)", receipt.FindProperty("NormalizedKey")!.GetColumnType());
        Assert.Equal("nvarchar(500)", receipt.FindProperty("OriginalNote")!.GetColumnType());
        var maximumIndexBytes = receipt.FindProperty("OwnerUserId")!.GetMaxLength()!.Value * 2
            + receipt.FindProperty("NormalizedKey")!.GetMaxLength()!.Value;
        Assert.Equal(1028, maximumIndexBytes);
        Assert.True(maximumIndexBytes <= 1700);
        Assert.Equal("Latin1_General_100_BIN2", receipt.FindProperty("NormalizedKey")!.GetCollation());
        Assert.Equal("char(64)", receipt.FindProperty("RequestHash")!.GetColumnType());
        Assert.Equal(18, receipt.FindProperty("OriginalAmount")!.GetPrecision());
        Assert.Equal(2, receipt.FindProperty("OriginalAmount")!.GetScale());
        Assert.Equal(500, receipt.FindProperty("OriginalNote")!.GetMaxLength());

        var primaryKey = Assert.Single(receipt.GetKeys());
        Assert.True(primaryKey.IsClustered());
        Assert.Equal(new[] { "Id" }, primaryKey.Properties.Select(property => property.Name));
        var ownerIndex = Assert.Single(receipt.GetIndexes(), index => index.GetDatabaseName() == "UX_ExpenseIdempotencyReceipts_Owner_Key");
        Assert.True(ownerIndex.IsUnique);
        Assert.False(ownerIndex.IsClustered());
        Assert.Equal(new[] { "OwnerUserId", "NormalizedKey" }, ownerIndex.Properties.Select(property => property.Name));
        var expenseForeignKey = Assert.Single(receipt.GetForeignKeys());
        Assert.Equal("Expenses", expenseForeignKey.PrincipalEntityType.GetTableName());
        Assert.Equal(DeleteBehavior.NoAction, expenseForeignKey.DeleteBehavior);
        Assert.Equal(new[] { "ExpenseId" }, expenseForeignKey.Properties.Select(property => property.Name));
    }
}
