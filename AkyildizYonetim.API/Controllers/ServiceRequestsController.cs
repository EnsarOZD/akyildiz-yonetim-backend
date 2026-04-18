using AkyildizYonetim.Application.ServiceRequests.Commands.CreateServiceRequest;
using AkyildizYonetim.Application.ServiceRequests.Commands.UpdateServiceRequestStatus;
using AkyildizYonetim.Application.ServiceRequests.Commands.AssignPersonnel;
using AkyildizYonetim.Application.ServiceRequests.Commands.ResolveRequest;
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
    private readonly IWebHostEnvironment _env;

    public ServiceRequestsController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var result = await _mediator.Send(new GetServiceRequestsQuery(status));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] string title, 
        [FromForm] string description, 
        [FromForm] ServiceRequestCategory category, 
        IFormFile? attachment)
    {
        string? attachmentUrl = null;
        if (attachment != null)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "service-requests");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(stream);
            }

            attachmentUrl = $"/uploads/service-requests/{fileName}";
        }

        var result = await _mediator.Send(new CreateServiceRequestCommand(title, description, category, attachmentUrl));
        return result.IsSuccess ? Ok(new { id = result.Data }) : BadRequest(result.ErrorMessage);
    }

    [HttpPatch("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignPersonnelRequest body)
    {
        var result = await _mediator.Send(new AssignPersonnelCommand(id, body.PersonnelId));
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [HttpPatch("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveRequestRequest body)
    {
        var result = await _mediator.Send(new ResolveServiceRequestCommand(id, body.ResolutionNote));
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
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
public record AssignPersonnelRequest(Guid PersonnelId);
public record ResolveRequestRequest(string ResolutionNote);
public record UpdateStatusRequest(ServiceRequestStatus Status, string? AdminNote);
