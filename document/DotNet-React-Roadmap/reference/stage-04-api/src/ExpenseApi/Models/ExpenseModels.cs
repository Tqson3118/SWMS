using System.Text.Json.Serialization;

namespace ExpenseApi.Models;

public sealed record ExpenseResponse(
    int Id,
    int CategoryId,
    string CategoryName,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal Amount,
    DateOnly SpentOn,
    string? Note
);

public sealed record CreateExpenseRequest(
    int CategoryId,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Amount,
    DateOnly? SpentOn,
    string? Note
);

public sealed record UpdateExpenseRequest(
    int CategoryId,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Amount,
    DateOnly? SpentOn,
    string? Note
);

public sealed record CategoryResponse(
    int Id,
    string Name
);

public sealed record CategoryMonthlySummary(
    int CategoryId,
    string CategoryName,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal Total,
    int Count
);

public sealed record MonthlySummaryResponse(
    int Year,
    int Month,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal GrandTotal,
    int TotalCount,
    IReadOnlyList<CategoryMonthlySummary> Categories
);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
