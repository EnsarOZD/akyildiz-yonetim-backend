using AkyildizYonetim.Application.Reports.Queries.GetFinancialReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "TenantRead")]
    [HttpGet("tenant/financial")]
    [HttpGet("financial")]
    public async Task<IActionResult> GetTenantFinancialReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? ownerId)
    {
        // This endpoint should only return tenant-specific data (debts, payments)
        var result = await _mediator.Send(new GetFinancialReportQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
            EndDate = endDate ?? DateTime.UtcNow,
            TenantId = tenantId,
            OwnerId = ownerId
            // Note: In a real scenario, the QueryHandler should be updated 
            // to ignore management expenses if called for a tenant report.
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "FinanceRead")]
    [HttpGet("finance/summary")]
    public async Task<IActionResult> GetFinanceSummaryReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        // This endpoint returns global financial metrics / profit-loss
        var result = await _mediator.Send(new GetFinancialReportQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
            EndDate = endDate ?? DateTime.UtcNow
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
} 