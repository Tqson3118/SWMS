namespace ExpenseApi.Tests;

using System.Net;
using System.Net.Http.Json;
using ExpenseApi.Models;
using ExpenseApi.Data;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[Collection("API")]
public sealed class ExpensesControllerTests(ApiTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task GetExpenses_WithDefaultPagination_ReturnsPagedResultAndCategoryName()
    {
        var response = await _client.GetAsync("/api/v1/expenses");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.Contains("X-Total-Count"));
        Assert.True(response.Headers.Contains("X-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));

        var result = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(result);
        Assert.True(int.Parse(response.Headers.GetValues("X-Total-Count").Single()) >= 3);
        Assert.Equal(1, int.Parse(response.Headers.GetValues("X-Page").Single()));
        Assert.NotEmpty(result);

        Assert.Contains(result, x => x.CategoryName == "Ăn uống");
    }

    [Fact]
    public async Task GetExpenses_WithPaginationAndDateFilter_ReturnsFilteredSubset()
    {
        // Jan 2026 has 2 expenses in fixture (50.00 and 75.50)
        var response = await _client.GetAsync("/api/v1/expenses?page=1&pageSize=1&from=2026-01-01&to=2026-01-31");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page1 = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(page1);
        Assert.Equal(2, int.Parse(response.Headers.GetValues("X-Total-Count").Single()));
        Assert.Equal(2, int.Parse(response.Headers.GetValues("X-Total-Pages").Single()));
        Assert.Single(page1);

        var response2 = await _client.GetAsync("/api/v1/expenses?page=2&pageSize=1&from=2026-01-01&to=2026-01-31");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var page2 = await response2.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(page2);
        Assert.Single(page2);

        Assert.NotEqual(page1[0].Id, page2[0].Id);
    }

    [Fact]
    public async Task GetExpenses_InvalidPageOrPageSize_ReturnsProblemDetails()
    {
        var responseZeroPage = await _client.GetAsync("/api/v1/expenses?page=0");
        Assert.Equal(HttpStatusCode.BadRequest, responseZeroPage.StatusCode);
        var problem1 = await responseZeroPage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem1);
        Assert.True(problem1.Errors.ContainsKey("page"));

        var responseLargePageSize = await _client.GetAsync("/api/v1/expenses?pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, responseLargePageSize.StatusCode);
        var problem2 = await responseLargePageSize.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem2);
        Assert.True(problem2.Errors.ContainsKey("pageSize"));

        var responseInvalidRange = await _client.GetAsync("/api/v1/expenses?from=2026-02-01&to=2026-01-01");
        Assert.Equal(HttpStatusCode.BadRequest, responseInvalidRange.StatusCode);
        var problem3 = await responseInvalidRange.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem3);
        Assert.True(problem3.Errors.ContainsKey("dateRange"));
    }

    [Fact]
    public async Task GetSummary_ValidYearAndMonth_ReturnsCategoryBreakdownAndTotals()
    {
        var response = await _client.GetAsync("/api/v1/expenses/summary?year=2026&month=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<MonthlySummaryResponse>();
        Assert.NotNull(summary);
        Assert.Equal(2026, summary.Year);
        Assert.Equal(1, summary.Month);
        Assert.Equal(125.50m, summary.GrandTotal);
        Assert.Equal(2, summary.TotalCount);

        var foodCategory = Assert.Single(summary.Categories);
        Assert.Equal("Ăn uống", foodCategory.CategoryName);
        Assert.Equal(125.50m, foodCategory.Total);
        Assert.Equal(2, foodCategory.Count);
    }

    [Fact]
    public async Task GetSummary_InvalidMonth_ReturnsProblemDetails()
    {
        var response = await _client.GetAsync("/api/v1/expenses/summary?year=2026&month=13");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("month"));
    }

    [Fact]
    public async Task PostExpense_ValidData_Returns201CreatedAndPersists()
    {
        var request = new CreateExpenseRequest(
            fixture.ShoppingCategoryId,
            89.90m,
            new DateOnly(2026, 3, 15),
            "Sách Clean Architecture");

        var response = await _client.PostAsJsonAsync("/api/v1/expenses", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(fixture.ShoppingCategoryId, created.CategoryId);
        Assert.Equal("Mua sắm", created.CategoryName);
        Assert.Equal(89.90m, created.Amount);
        Assert.Equal(new DateOnly(2026, 3, 15), created.SpentOn);
        Assert.Equal("Sách Clean Architecture", created.Note);

        // Verify get by id
        var getResponse = await _client.GetAsync($"/api/v1/expenses/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Mua sắm", retrieved.CategoryName);
    }

    [Fact]
    public async Task PostExpense_InvalidAmountOrDefaultDate_ReturnsValidationProblemDetails()
    {
        var negativeReq = new CreateExpenseRequest(
            fixture.FoodCategoryId,
            -15.00m,
            new DateOnly(2026, 3, 1),
            "Negative amount");
        var resNeg = await _client.PostAsJsonAsync("/api/v1/expenses", negativeReq);
        Assert.Equal(HttpStatusCode.BadRequest, resNeg.StatusCode);
        var problemNeg = await resNeg.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemNeg);
        Assert.True(problemNeg.Errors.ContainsKey("amount"));

        var defaultDateReq = new CreateExpenseRequest(
            fixture.FoodCategoryId,
            50m,
            default,
            "Default date");
        var resDate = await _client.PostAsJsonAsync("/api/v1/expenses", defaultDateReq);
        Assert.Equal(HttpStatusCode.BadRequest, resDate.StatusCode);
        var problemDate = await resDate.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDate);
        Assert.True(problemDate.Errors.ContainsKey("spentOn"));
    }

    [Fact]
    public async Task PostExpense_NonExistentCategoryId_ReturnsValidationProblemDetails()
    {
        var nonExistentCategoryReq = new CreateExpenseRequest(
            999999,
            50.00m,
            new DateOnly(2026, 3, 1),
            "Invalid category");

        var response = await _client.PostAsJsonAsync("/api/v1/expenses", nonExistentCategoryReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("categoryId"));
    }

    [Fact]
    public async Task DeleteExpense_ExistingId_Returns204NoContent_Then404OnSecondDelete()
    {
        // First create an expense to delete
        var req = new CreateExpenseRequest(
            fixture.FoodCategoryId,
            15.00m,
            new DateOnly(2026, 4, 1),
            "To be deleted");
        var createRes = await _client.PostAsJsonAsync("/api/v1/expenses", req);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(created);

        // Delete it
        var deleteRes = await _client.DeleteAsync($"/api/v1/expenses/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // Second delete should be 404
        var secondDeleteRes = await _client.DeleteAsync($"/api/v1/expenses/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteRes.StatusCode);
        var notFoundProblem = await secondDeleteRes.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(notFoundProblem);
        Assert.Equal((int)HttpStatusCode.NotFound, notFoundProblem.Status);

        // Get by id should also return 404
        var getRes = await _client.GetAsync($"/api/v1/expenses/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }

    [Fact]
    public async Task PutExpense_ValidData_Returns200AndPersistsChanges()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 15m, new DateOnly(2026, 4, 1), "before"));
        var created = await create.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(created);

        var update = await _client.PutAsJsonAsync($"/api/v1/expenses/{created.Id}", new UpdateExpenseRequest(
            fixture.TravelCategoryId, 0m, DateOnly.MinValue, "after"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var changed = await update.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(changed);
        Assert.Equal(0m, changed.Amount);
        Assert.Equal(DateOnly.MinValue, changed.SpentOn);
        Assert.Equal("after", changed.Note);

        var get = await _client.GetAsync($"/api/v1/expenses/{created.Id}");
        var persisted = await get.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(persisted);
        Assert.Equal(fixture.TravelCategoryId, persisted.CategoryId);
        Assert.Equal(DateOnly.MinValue, persisted.SpentOn);
    }

    [Fact]
    public async Task PutExpense_MissingDate_Returns400AndPreservesOriginal()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 15m, new DateOnly(2026, 4, 1), "before"));
        var created = await create.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(created);

        var update = await _client.PutAsJsonAsync($"/api/v1/expenses/{created.Id}", new { categoryId = fixture.TravelCategoryId, amount = 99m, note = "bad" });
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);

        var get = await _client.GetAsync($"/api/v1/expenses/{created.Id}");
        var persisted = await get.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(persisted);
        Assert.Equal(15m, persisted.Amount);
        Assert.Equal(new DateOnly(2026, 4, 1), persisted.SpentOn);
        Assert.Equal("before", persisted.Note);
    }

    [Fact]
    public async Task PutExpense_UnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/expenses/999999", new UpdateExpenseRequest(
            fixture.FoodCategoryId, 1m, new DateOnly(2026, 4, 1), null));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostExpense_ZeroAmountAndDateMinimumAreValid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 0m, DateOnly.MinValue, "boundary"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostExpense_Note500AndTwoDecimalAmountAreValid_ButNote501AndThreeDecimalsFail()
    {
        var valid = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 12.34m, new DateOnly(2026, 5, 1), new string('x', 500)));
        Assert.Equal(HttpStatusCode.Created, valid.StatusCode);

        var longNote = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 12.34m, new DateOnly(2026, 5, 2), new string('x', 501)));
        Assert.Equal(HttpStatusCode.BadRequest, longNote.StatusCode);

        var precision = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 12.345m, new DateOnly(2026, 5, 3), null));
        Assert.Equal(HttpStatusCode.BadRequest, precision.StatusCode);
    }

    [Fact]
    public async Task MoneyTransport_UsesExactStringsAndAcceptsLegacyNumbers_WhileRejectingOverflow()
    {
        const string maximum = "9999999999999999.99";
        using var stringRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/expenses")
        {
            Content = new StringContent($"{{\"categoryId\":{fixture.FoodCategoryId},\"amount\":\"125.50\",\"spentOn\":\"2026-07-01\",\"note\":null}}", System.Text.Encoding.UTF8, "application/json")
        };
        var stringResponse = await _client.SendAsync(stringRequest);
        Assert.Equal(HttpStatusCode.Created, stringResponse.StatusCode);
        using var stringJson = System.Text.Json.JsonDocument.Parse(await stringResponse.Content.ReadAsStringAsync());
        Assert.Equal(System.Text.Json.JsonValueKind.String, stringJson.RootElement.GetProperty("amount").ValueKind);
        Assert.Equal("125.50", stringJson.RootElement.GetProperty("amount").GetString());

        var numericResponse = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(fixture.FoodCategoryId, 0.01m, new DateOnly(2026, 7, 2), null));
        Assert.Equal(HttpStatusCode.Created, numericResponse.StatusCode);

        var maxOne = await _client.PostAsync("/api/v1/expenses", new StringContent($"{{\"categoryId\":{fixture.FoodCategoryId},\"amount\":\"{maximum}\",\"spentOn\":\"2026-07-03\",\"note\":null}}", System.Text.Encoding.UTF8, "application/json"));
        var maxTwo = await _client.PostAsync("/api/v1/expenses", new StringContent($"{{\"categoryId\":{fixture.FoodCategoryId},\"amount\":\"{maximum}\",\"spentOn\":\"2026-07-04\",\"note\":null}}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, maxOne.StatusCode);
        Assert.Equal(HttpStatusCode.Created, maxTwo.StatusCode);
        var summary = await _client.GetAsync("/api/v1/expenses/summary?year=2026&month=7");
        Assert.Equal(HttpStatusCode.OK, summary.StatusCode);
        using var summaryJson = System.Text.Json.JsonDocument.Parse(await summary.Content.ReadAsStringAsync());
        Assert.Equal(System.Text.Json.JsonValueKind.String, summaryJson.RootElement.GetProperty("grandTotal").ValueKind);
        Assert.Equal("20000000000000125.49", summaryJson.RootElement.GetProperty("grandTotal").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.String, summaryJson.RootElement.GetProperty("categories")[0].GetProperty("total").ValueKind);

        var overflow = await _client.PostAsync("/api/v1/expenses", new StringContent($"{{\"categoryId\":{fixture.FoodCategoryId},\"amount\":\"10000000000000000.00\",\"spentOn\":\"2026-07-05\",\"note\":null}}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, overflow.StatusCode);
        var precision = await _client.PostAsync("/api/v1/expenses", new StringContent($"{{\"categoryId\":{fixture.FoodCategoryId},\"amount\":\"1.001\",\"spentOn\":\"2026-07-06\",\"note\":null}}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, precision.StatusCode);
    }

    [Fact]
    public async Task PutExpense_UnknownCategory_Returns400AndPreservesOriginal()
    {
        var create = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 15m, new DateOnly(2026, 5, 4), "before"));
        var created = await create.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(created);
        var update = await _client.PutAsJsonAsync($"/api/v1/expenses/{created.Id}", new UpdateExpenseRequest(
            999999, 99m, new DateOnly(2026, 5, 5), "bad"));
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);
        var get = await _client.GetAsync($"/api/v1/expenses/{created.Id}");
        var persisted = await get.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(persisted);
        Assert.Equal(fixture.FoodCategoryId, persisted.CategoryId);
        Assert.Equal(15m, persisted.Amount);
        Assert.Equal("before", persisted.Note);
    }

    [Fact]
    public async Task GetExpenses_SameDateUsesIdDescendingAndPageOffsetOverflowIs400()
    {
        var first = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 1m, new DateOnly(2026, 6, 1), "first"));
        var second = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 2m, new DateOnly(2026, 6, 1), "second"));
        var firstCreated = await first.Content.ReadFromJsonAsync<ExpenseResponse>();
        var secondCreated = await second.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.NotNull(firstCreated);
        Assert.NotNull(secondCreated);
        var page = await _client.GetAsync("/api/v1/expenses?page=1&pageSize=1&from=2026-06-01&to=2026-06-01");
        var items = await page.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(items);
        Assert.Equal(secondCreated.Id, items[0].Id);
        var overflow = await _client.GetAsync("/api/v1/expenses?page=2147483647&pageSize=100");
        Assert.Equal(HttpStatusCode.BadRequest, overflow.StatusCode);
    }

    [Fact]
    public async Task SummarySupportsDateOnlyMinimumAndMaximumYears()
    {
        var min = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 1m, DateOnly.MinValue, "min"));
        var max = await _client.PostAsJsonAsync("/api/v1/expenses", new CreateExpenseRequest(
            fixture.FoodCategoryId, 2m, DateOnly.MaxValue, "max"));
        Assert.Equal(HttpStatusCode.Created, min.StatusCode);
        Assert.Equal(HttpStatusCode.Created, max.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/v1/expenses/summary?year=1&month=1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/v1/expenses/summary?year=9999&month=12")).StatusCode);
    }

    [Fact]
    public async Task PostExpense_NullBodyAndInvalidDateReturnStructured400()
    {
        var nullBody = new HttpRequestMessage(HttpMethod.Post, "/api/v1/expenses")
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };
        var nullResponse = await _client.SendAsync(nullBody);
        Assert.Equal(HttpStatusCode.BadRequest, nullResponse.StatusCode);
        Assert.Contains("errors", await nullResponse.Content.ReadAsStringAsync());

        var invalidDate = new HttpRequestMessage(HttpMethod.Post, "/api/v1/expenses")
        {
            Content = new StringContent($"{{\"categoryId\":{fixture.FoodCategoryId},\"amount\":1,\"spentOn\":\"not-a-date\"}}", System.Text.Encoding.UTF8, "application/json")
        };
        var invalidResponse = await _client.SendAsync(invalidDate);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        Assert.Contains("errors", await invalidResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UnexpectedProviderFailure_ReturnsSafeProblemDetailsWithoutStackOrSecret()
    {
        var faultDatabase = ApiDatabase.NewName();
        Environment.SetEnvironmentVariable("EXPENSE_API_DB_NAME", faultDatabase);
        var provider = new TestLoggerProvider();
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders().AddProvider(provider));
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(x => x.ServiceType == typeof(IExpenseService));
                services.Remove(descriptor);
                services.AddScoped<IExpenseService, ThrowingExpenseService>();
            });
        });
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("should-not-leak", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", body, StringComparison.OrdinalIgnoreCase);
        using var json = System.Text.Json.JsonDocument.Parse(body);
        var traceId = json.RootElement.GetProperty("traceId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(traceId));
        Assert.Contains(provider.Messages, message => message.Contains(traceId!, StringComparison.Ordinal));
        Console.WriteLine($"StructuredLogTrace={traceId}; message={provider.Messages.Single(message => message.Contains(traceId!, StringComparison.Ordinal))}");
    }

    [Fact]
    public async Task EfMigrations_AreAppliedAndSeededRowsSurvive()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
        var migrations = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
        Assert.Contains(migrations, x => x.EndsWith("_InitialCreate", StringComparison.Ordinal));
        Assert.Contains(migrations, x => x.EndsWith("_AddExpenseCreatedAtUtc", StringComparison.Ordinal));
        var count = await db.Expenses.CountAsync();
        Assert.True(count >= 3);
        Assert.True(await db.Expenses.AnyAsync(x => x.CreatedAtUtc.HasValue));
        Console.WriteLine($"EF database={fixture.DatabaseName}; migrations={string.Join(',', migrations)}; seededRows={count}");
    }

    [Fact]
    public async Task AddCreatedAtMigration_PreservesRowCreatedAfterInitialMigration()
    {
        var databaseName = ApiDatabase.NewName();
        await ApiDatabase.CreateFreshDatabaseAsync(databaseName, Environment.GetEnvironmentVariable("DN45_API779_DB_LOG"));
        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseSqlServer(ApiDatabase.ConnectionString(databaseName)).Options;
        await using var db = new ExpenseDbContext(options);
        var initial = db.Database.GetMigrations().First();
        await db.Database.MigrateAsync(initial);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Categories(Name) VALUES (N'Preserved'); INSERT INTO Expenses(CategoryId, Amount, SpentOn, Note) VALUES (1, 0, '0001-01-01', N'before add column');");
        await db.Database.MigrateAsync();
        var preserved = await db.Expenses.AsNoTracking().SingleAsync(x => x.Note == "before add column");
        Assert.Equal(0m, preserved.Amount);
        Assert.Equal(DateOnly.MinValue, preserved.SpentOn);
        Assert.Null(preserved.CreatedAtUtc);
        Console.WriteLine($"Migration row preservation database={databaseName}; initial={initial}; final={string.Join(',', await db.Database.GetAppliedMigrationsAsync())}");
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        var response = await _client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Name == "Ăn uống");
        Assert.Contains(categories, c => c.Name == "Đi lại");
        Assert.Contains(categories, c => c.Name == "Mua sắm");
    }
}

internal sealed class ThrowingExpenseService : IExpenseService
{
    private static Exception Failure() => new InvalidOperationException("Injected provider outage secret=should-not-leak");
    private static Task<T> Fail<T>() => Task.FromException<T>(Failure());
    public Task<PagedResult<ExpenseResponse>> GetExpensesAsync(int page, int pageSize, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default) => Fail<PagedResult<ExpenseResponse>>();
    public Task<ExpenseResponse?> GetExpenseByIdAsync(int id, CancellationToken cancellationToken = default) => Fail<ExpenseResponse?>();
    public Task<MonthlySummaryResponse> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default) => Fail<MonthlySummaryResponse>();
    public Task<ExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default) => Fail<ExpenseResponse>();
    public Task<ExpenseResponse?> UpdateExpenseAsync(int id, UpdateExpenseRequest request, CancellationToken cancellationToken = default) => Fail<ExpenseResponse?>();
    public Task<bool> DeleteExpenseAsync(int id, CancellationToken cancellationToken = default) => Fail<bool>();
    public Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default) => Fail<IReadOnlyList<CategoryResponse>>();
    public Task<CategoryResponse> AddCategoryAsync(string name, CancellationToken cancellationToken = default) => Fail<CategoryResponse>();
}

internal sealed class TestLoggerProvider : ILoggerProvider
{
    public List<string> Messages { get; } = [];
    public ILogger CreateLogger(string categoryName) => new TestLogger(Messages, categoryName);
    public void Dispose() { }
}

internal sealed class TestLogger(List<string> messages, string category) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (messages) messages.Add($"{category} {formatter(state, exception)}");
    }
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
