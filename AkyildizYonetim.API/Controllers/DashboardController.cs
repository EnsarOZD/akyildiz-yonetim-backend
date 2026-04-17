using AkyildizYonetim.Application.Dashboard.Queries.GetDebtsSummary;
using AkyildizYonetim.Application.Dashboard.Queries.GetDashboardSummary;
using AkyildizYonetim.Application.Dashboard.Queries.GetOwnerDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "FinanceRead")]
    [HttpGet("debts-summary")]
    public async Task<IActionResult> GetDebtsSummary([FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetDebtsSummaryQuery(year));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "FinanceRead")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? dateFilter)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(dateFilter));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "OwnerAccess")]
    [HttpGet("owner")]
    public async Task<IActionResult> GetOwnerDashboard()
    {
        var result = await _mediator.Send(new GetOwnerDashboardQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
}
