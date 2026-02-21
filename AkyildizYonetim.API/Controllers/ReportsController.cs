using AkyildizYonetim.Application.Reports.Queries.GetFinancialReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? ownerId)
    {
        var result = await _mediator.Send(new GetFinancialReportQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
            EndDate = endDate ?? DateTime.UtcNow,
            TenantId = tenantId,
            OwnerId = ownerId
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
} 