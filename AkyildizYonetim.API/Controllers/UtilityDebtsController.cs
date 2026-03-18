using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt.CreateAidatForPeriod;
using AkyildizYonetim.Application.UtilityDebts.Commands.UpdateUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Commands.DeleteUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebts;
using AkyildizYonetim.Application.UtilityDebts.Commands.ImportUtilityDebts;
using AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebtById;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.UtilityDebts.Commands.DistributeSharedExpense;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AkyildizYonetim.Application.UtilityDebts.Commands.DeleteBulkUtilityDebts;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/[controller]")]
public class UtilityDebtsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UtilityDebtsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("import")]
    public async Task<IActionResult> ImportUtilityDebts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya bulunamadı.");

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new ImportUtilityDebtsFromExcelCommand { ExcelStream = stream });
        
        return result.IsSuccess ? Ok(result) : BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebts.UtilityDebtDto>), 200)]
    public async Task<IActionResult> GetUtilityDebts(
        [FromQuery] Guid? flatId, 
        [FromQuery] DebtType? type, 
        [FromQuery] int? periodYear, 
        [FromQuery] int? periodMonth, 
        [FromQuery] DebtStatus? status, 
        [FromQuery] bool? excludePaid,
        [FromQuery] Guid? tenantId, 
        [FromQuery] Guid? ownerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetUtilityDebtsQuery 
        { 
            FlatId = flatId, 
            Type = type, 
            PeriodYear = periodYear, 
            PeriodMonth = periodMonth, 
            Status = status, 
            ExcludePaid = excludePaid,
            TenantId = tenantId, 
            OwnerId = ownerId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUtilityDebtById(Guid id)
    {
        var result = await _mediator.Send(new GetUtilityDebtByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("last-readings/{type}")]
    public Task<IActionResult> GetLastReadings(string type)
    {
        // Bu endpoint son okumaları getirmek için kullanılacak
        // Şimdilik boş liste döndürüyoruz, daha sonra implement edilecek
        return Task.FromResult<IActionResult>(Ok(new List<object>()));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost]
    public async Task<IActionResult> CreateUtilityDebt([FromBody] CreateUtilityDebtCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkUtilityDebts([FromBody] CreateBulkUtilityDebtsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUtilityDebt(Guid id, [FromBody] UpdateUtilityDebtCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUtilityDebt(Guid id)
    {
        var result = await _mediator.Send(new DeleteUtilityDebtCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> DeleteBulkUtilityDebts([FromBody] DeleteBulkUtilityDebtsRequest request)
    {
        if (request?.Ids == null || !request.Ids.Any()) return BadRequest("Lütfen silinecek kayıtları seçin.");
        
        var command = new DeleteBulkUtilityDebtsCommand { Ids = request.Ids };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchUtilityDebt(Guid id, [FromBody] AkyildizYonetim.Application.UtilityDebts.Commands.PatchUtilityDebt.PatchUtilityDebtCommand command)
    {
        var fixedCommand = command with { Id = id };
        var result = await _mediator.Send(fixedCommand);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpDelete("period/{period}")]
    public Task<IActionResult> DeleteByPeriod(string period)
    {
        // Bu endpoint dönem bazında silme için kullanılacak
        // Şimdilik başarılı döndürüyoruz, daha sonra implement edilecek
        return Task.FromResult<IActionResult>(NoContent());
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("distribute")]
    public async Task<IActionResult> DistributeSharedExpense([FromBody] DistributeSharedExpenseRequest request)
    {
        var result = await _mediator.Send(new DistributeSharedExpenseCommand 
        { 
            Period = request.Period, 
            UtilityType = request.UtilityType 
        });

        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("create-aidat")]
    public async Task<IActionResult> CreateAidat([FromBody] CreateAidatRequest request, CancellationToken ct)
    {
        var (tenantDuesCreated, ownerDuesCreated) =
            await _mediator.Send(new CreateAidatForPeriodCommand(request.Period, request.DueDate), ct);

        return Ok(new { tenantDuesCreated, ownerDuesCreated });
    }
}

public class DistributeSharedExpenseRequest
{
    public string Period { get; set; } = string.Empty;
    public string UtilityType { get; set; } = string.Empty;
}

public class DeleteBulkUtilityDebtsRequest
{
    public List<Guid> Ids { get; set; } = new();
}

public sealed class CreateAidatRequest
{
    // "YYYY-MM" (ör. 2025-03)
    public string Period { get; set; } = default!;
    // Son ödeme tarihi (zorunlu)
    public DateTime DueDate { get; set; }
}