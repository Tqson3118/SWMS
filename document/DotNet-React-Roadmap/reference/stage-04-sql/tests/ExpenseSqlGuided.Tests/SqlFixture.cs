using ExpenseSqlGuided;

namespace ExpenseSqlGuided.Tests;

public sealed class SqlFixture : IAsyncLifetime
{
    public string DatabaseName { get; } = SqlSchema.NewDatabaseName();
    public SqlExpenseStore Store { get; private set; } = null!;
    public async Task InitializeAsync() { await SqlSchema.CreateDatabaseAsync(DatabaseName); await SqlSchema.CreateSchemaAsync(DatabaseName); Store = new SqlExpenseStore(DatabaseName); }
    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("SQL", DisableParallelization = true)] public sealed class SqlCollection : ICollectionFixture<SqlFixture> { }
