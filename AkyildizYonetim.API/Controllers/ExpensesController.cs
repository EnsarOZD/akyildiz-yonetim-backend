using AkyildizYonetim.Application.Expenses.Commands.CreateExpense;
using AkyildizYonetim.Application.Expenses.Commands.DeleteExpense;
using AkyildizYonetim.Application.Expenses.Commands.UpdateExpense;
using AkyildizYonetim.Application.Expenses.Queries.GetExpenseById;
using AkyildizYonetim.Application.Expenses.Queries.GetExpenses;
using AkyildizYonetim.Application.Expenses.Queries.GetExpenseStats;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "FinanceRead")]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] ExpenseType? type,
        [FromQuery] Guid? ownerId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetExpensesQuery
        {
            Type = type,
            OwnerId = ownerId,
            StartDate = startDate,
            EndDate = endDate,
            SearchTerm = searchTerm
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpenseById(Guid id)
    {
        var result = await _mediator.Send(new GetExpenseByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetExpenseStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _mediator.Send(new GetExpenseStatsQuery
        {
            StartDate = startDate,
            EndDate = endDate
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "FinanceWrite")]
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] JsonElement jsonElement)
    {
        // ... (remaining methods)
        try
        {
            Console.WriteLine($"🔍 Raw JSON: {jsonElement}");
            
            // JSON'dan manuel olarak command oluştur
            var command = new CreateExpenseCommand
            {
                Title = jsonElement.GetProperty("title").GetString() ?? string.Empty,
                Amount = jsonElement.GetProperty("amount").GetDecimal(),
                Type = Enum.Parse<ExpenseType>(jsonElement.GetProperty("type").GetString() ?? "Other"),
                ExpenseDate = jsonElement.GetProperty("expenseDate").GetDateTime(),
                Description = jsonElement.TryGetProperty("description", out var desc) ? desc.GetString() : null
            };

            Console.WriteLine($"🔍 Backend'e gelen veri: Title={command.Title}, Amount={command.Amount}, Type={command.Type}, Date={command.ExpenseDate}");
            
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                Console.WriteLine($"❌ Hata: {result.ErrorMessage}");
                if (result.Errors != null && result.Errors.Any())
                {
                    Console.WriteLine($"❌ Detaylar: {string.Join(", ", result.Errors)}");
                }
                return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
            }
            
            Console.WriteLine($"✅ Başarılı! ID: {result.Data}");
            return CreatedAtAction(nameof(GetExpenseById), new { id = result.Data }, result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ JSON parse hatası: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            return BadRequest($"JSON parse hatası: {ex.Message}");
        }
    }

    [Authorize(Policy = "FinanceWrite")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] JsonElement jsonElement)
    {
        try
        {
            Console.WriteLine($"🔍 Update Raw JSON: {jsonElement}");
            
            // JSON'dan manuel olarak command oluştur
            var command = new UpdateExpenseCommand
            {
                Id = id,
                Title = jsonElement.GetProperty("title").GetString() ?? string.Empty,
                Amount = jsonElement.GetProperty("amount").GetDecimal(),
                Type = Enum.Parse<ExpenseType>(jsonElement.GetProperty("type").GetString() ?? "Other"),
                ExpenseDate = jsonElement.GetProperty("expenseDate").GetDateTime(),
                Description = jsonElement.TryGetProperty("description", out var desc) ? desc.GetString() : null
            };

            Console.WriteLine($"🔍 Update Backend'e gelen veri: ID={command.Id}, Title={command.Title}, Amount={command.Amount}, Type={command.Type}, Date={command.ExpenseDate}");
            
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                Console.WriteLine($"❌ Update Hata: {result.ErrorMessage}");
                if (result.Errors != null && result.Errors.Any())
                {
                    Console.WriteLine($"❌ Update Detaylar: {string.Join(", ", result.Errors)}");
                }
                return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
            }
            
            Console.WriteLine($"✅ Update Başarılı! ID: {id}");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Update JSON parse hatası: {ex.Message}");
            Console.WriteLine($"❌ Update Stack trace: {ex.StackTrace}");
            return BadRequest($"JSON parse hatası: {ex.Message}");
        }
    }

    [Authorize(Policy = "FinanceWrite")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var result = await _mediator.Send(new DeleteExpenseCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
}

public class CreateExpenseRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
}

public class UpdateExpenseRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
} 