using ExpenseSqlGuided;

string databaseName = SqlSchema.NewDatabaseName();
await SqlSchema.CreateDatabaseAsync(databaseName);
await SqlSchema.CreateSchemaAsync(databaseName);
var store = new SqlExpenseStore(databaseName);
int food = await store.AddCategoryAsync("Ăn"); int travel = await store.AddCategoryAsync("Đi lại");
await store.AddExpenseAsync(food, 0m, new DateOnly(2026, 1, 1), null);
await store.AddExpenseAsync(food, 125.50m, new DateOnly(2026, 1, 2), "guided");
await store.AddExpenseAsync(travel, 20m, new DateOnly(2026, 2, 1), null);
Console.WriteLine($"Rows: {(await store.PageAsync(1, 10)).Count}");
Console.WriteLine($"January total: {(await store.MonthlyTotalsAsync()).Single(x => x.Month == 1).Total}");
