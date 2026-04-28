using AkyildizYonetim.Application.Owners.Commands.CreateOwner;
using AkyildizYonetim.Application.Owners.Commands.DeleteOwner;
using AkyildizYonetim.Application.Owners.Commands.UpdateOwner;
using AkyildizYonetim.Application.Owners.Queries.GetOwnerById;
using AkyildizYonetim.Application.Owners.Queries.GetOwners;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OwnersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerDto>), 200)]
    public async Task<IActionResult> GetOwners(
        [FromQuery] bool? isActive, 
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetOwnersQuery 
        { 
            IsActive = isActive, 
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOwnerById(Guid id)
    {
        var result = await _mediator.Send(new GetOwnerByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost]
    public async Task<IActionResult> CreateOwner([FromBody] CreateOwnerCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetOwnerById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOwner(Guid id, [FromBody] UpdateOwnerCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOwner(Guid id)
    {
        var result = await _mediator.Send(new DeleteOwnerCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("dues")]
    public IActionResult GetOwnerDues()
    {
        // For now, return empty array since this feature is not fully implemented
        return Ok(new List<object>());
    }
}

public class CreateOwnerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
}

public class UpdateOwnerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
} 