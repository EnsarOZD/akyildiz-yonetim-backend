using AkyildizYonetim.Application.Tenants.Commands.CreateTenant;
using AkyildizYonetim.Application.Tenants.Commands.DeleteTenant;
using AkyildizYonetim.Application.Tenants.Commands.UpdateTenant;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantById;
using AkyildizYonetim.Application.Tenants.Queries.GetTenants;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<IActionResult> GetTenants([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetTenantsQuery { IsActive = isActive, SearchTerm = searchTerm });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetTenantStats()
    {
        var result = await _mediator.Send(new GetTenantStatsQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetTenantById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var result = await _mediator.Send(new DeleteTenantCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public int FlatId { get; set; }
    public string RentStartDate { get; set; } = string.Empty;
    public string RentEndDate { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public decimal Deposit { get; set; }
}

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public int FlatId { get; set; }
    public string RentStartDate { get; set; } = string.Empty;
    public string RentEndDate { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public decimal Deposit { get; set; }
} 