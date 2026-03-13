using AkyildizYonetim.Application.AuditLogs.Queries.GetAuditLogs;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Roles = "admin,manager")]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] AuditEntityType? entityType,
        [FromQuery] string? userId,
        [FromQuery] AuditAction? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            EntityType = entityType,
            UserId = userId,
            Action = action,
            Page = page,
            PageSize = pageSize
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
} 