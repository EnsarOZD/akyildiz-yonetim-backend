using AkyildizYonetim.Application.ServiceRequests.Commands.CreateServiceRequest;
using AkyildizYonetim.Application.ServiceRequests.Commands.UpdateServiceRequestStatus;
using AkyildizYonetim.Application.ServiceRequests.Queries.GetServiceRequests;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var result = await _mediator.Send(new GetServiceRequestsQuery(status));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestRequest body)
    {
        var result = await _mediator.Send(new CreateServiceRequestCommand(body.Title, body.Description, body.Category));
        return result.IsSuccess ? Ok(new { id = result.Data }) : BadRequest(result.ErrorMessage);
    }

    [Authorize(Policy = "FinanceWrite")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest body)
    {
        var result = await _mediator.Send(new UpdateServiceRequestStatusCommand(id, body.Status, body.AdminNote));
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }
}

public record CreateServiceRequestRequest(string Title, string Description, ServiceRequestCategory Category);
public record UpdateStatusRequest(ServiceRequestStatus Status, string? AdminNote);
