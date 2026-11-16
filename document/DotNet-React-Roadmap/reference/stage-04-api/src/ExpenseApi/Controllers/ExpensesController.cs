namespace ExpenseApi.Controllers;

using ExpenseApi.Models;
using ExpenseApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/expenses")]
public sealed class ExpensesController(IExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponse>>> GetExpenses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        if (page < 1)
        {
            errors["page"] = ["Page phải lớn hơn hoặc bằng 1."];
        }

        if (pageSize < 1 || pageSize > 100)
        {
            errors["pageSize"] = ["PageSize phải từ 1 đến 100."];
        }

        if ((long)(page - 1) * pageSize > int.MaxValue)
        {
            errors["page"] = ["Page offset vượt giới hạn."];
        }

        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            errors["dateRange"] = ["Ngày bắt đầu không được sau ngày kết thúc."];
        }

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Tham số phân trang hoặc lọc ngày không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await expenseService.GetExpensesAsync(page, pageSize, from, to, cancellationToken);

        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        Response.Headers["X-Page"] = result.Page.ToString();
        Response.Headers["X-Page-Size"] = result.PageSize.ToString();
        Response.Headers["X-Total-Pages"] = result.TotalPages.ToString();

        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await expenseService.GetExpenseByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Khoản chi tiêu không tồn tại.",
                Detail = $"Không tìm thấy khoản chi tiêu với id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(item);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<MonthlySummaryResponse>> GetSummary(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        if (year < 1 || year > 9999)
        {
            errors["year"] = ["Year phải từ 1 đến 9999."];
        }

        if (month < 1 || month > 12)
        {
            errors["month"] = ["Month phải từ 1 đến 12."];
        }

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Tham số tháng và năm không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var summary = await expenseService.GetMonthlySummaryAsync(year, month, cancellationToken);
        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create(
        [FromBody] CreateExpenseRequest? request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["body"] = ["Request body là bắt buộc và phải là JSON object."]
            }) { Status = StatusCodes.Status400BadRequest });
        }

        if (request.Amount < 0)
        {
            errors["amount"] = ["Số tiền phải lớn hơn hoặc bằng 0."];
        }

        if (!request.SpentOn.HasValue)
        {
            errors["spentOn"] = ["Ngày chi tiêu không hợp lệ."];
        }

        if (request.CategoryId <= 0)
        {
            errors["categoryId"] = ["CategoryId phải lớn hơn 0."];
        }

        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Dữ liệu chi tiêu không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var created = await expenseService.CreateExpenseAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["categoryId"] = [ex.Message]
            })
            {
                Title = "Danh mục không tồn tại.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["general"] = [ex.Message]
            })
            {
                Title = "Dữ liệu không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> Update(
        int id,
        [FromBody] UpdateExpenseRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["body"] = ["Request body là bắt buộc và phải là JSON object."]
            }) { Status = StatusCodes.Status400BadRequest });
        }
        var errors = ValidateUpdate(request);
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Dữ liệu chi tiêu không hợp lệ.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var updated = await expenseService.UpdateExpenseAsync(id, request, cancellationToken);
            if (updated is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Khoản chi tiêu không tồn tại.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["categoryId"] = [ex.Message]
            })
            {
                Title = "Danh mục không tồn tại.",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        bool deleted = await expenseService.DeleteExpenseAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Khoản chi tiêu không tồn tại.",
                Detail = $"Không tìm thấy khoản chi tiêu với id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }

    private static Dictionary<string, string[]> ValidateUpdate(UpdateExpenseRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.Amount < 0) errors["amount"] = ["Số tiền phải lớn hơn hoặc bằng 0."];
        if (!request.SpentOn.HasValue) errors["spentOn"] = ["Ngày chi tiêu là bắt buộc."];
        if (request.CategoryId <= 0) errors["categoryId"] = ["CategoryId phải lớn hơn 0."];
        if (request.Note is not null && request.Note.Length > 500) errors["note"] = ["Ghi chú tối đa 500 ký tự."];
        return errors;
    }
}
