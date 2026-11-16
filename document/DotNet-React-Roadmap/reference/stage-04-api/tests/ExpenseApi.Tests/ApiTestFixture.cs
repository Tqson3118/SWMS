namespace ExpenseApi.Tests;

using ExpenseApi.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public sealed class ApiTestFixture : IAsyncLifetime
{
    public string DatabaseName { get; } = ApiDatabase.NewName();
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public int FoodCategoryId { get; private set; }
    public int TravelCategoryId { get; private set; }
    public int ShoppingCategoryId { get; private set; }

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("EXPENSE_API_DB_NAME", DatabaseName);
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("DatabaseName", DatabaseName);
            builder.ConfigureServices(services =>
            {
            });
        });

        Client = Factory.CreateClient();

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
        var food = new Category { Name = "Ăn uống" };
        var travel = new Category { Name = "Đi lại" };
        var shopping = new Category { Name = "Mua sắm" };
        db.Categories.AddRange(food, travel, shopping);
        await db.SaveChangesAsync();
        FoodCategoryId = food.Id;
        TravelCategoryId = travel.Id;
        ShoppingCategoryId = shopping.Id;
        db.Expenses.AddRange(
            new Expense { CategoryId = FoodCategoryId, Amount = 50.00m, SpentOn = new DateOnly(2026, 1, 10), Note = "Ăn trưa", CreatedAtUtc = DateTime.UtcNow },
            new Expense { CategoryId = FoodCategoryId, Amount = 75.50m, SpentOn = new DateOnly(2026, 1, 15), Note = "Ăn tối", CreatedAtUtc = DateTime.UtcNow },
            new Expense { CategoryId = TravelCategoryId, Amount = 20.00m, SpentOn = new DateOnly(2026, 2, 5), Note = "Xe buýt", CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }
}

[CollectionDefinition("API", DisableParallelization = true)]
public sealed class ApiCollection : ICollectionFixture<ApiTestFixture> { }
