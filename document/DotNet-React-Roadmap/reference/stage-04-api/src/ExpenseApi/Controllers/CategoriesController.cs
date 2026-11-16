namespace ExpenseApi.Controllers;

using ExpenseApi.Models;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController(IExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories(
        CancellationToken cancellationToken = default)
    {
        var categories = await expenseService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    public sealed record CreateCategoryRequest(string Name);

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["name"] = ["Tên danh mục không được để trống."]
            })
            {
                Title = "Dữ liệu danh mục không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var created = await expenseService.AddCategoryAsync(request.Name, cancellationToken);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["name"] = [ex.Message]
            })
            {
                Title = "Tên danh mục không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
