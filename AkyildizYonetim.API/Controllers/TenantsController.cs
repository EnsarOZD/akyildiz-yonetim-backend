using AkyildizYonetim.Application.Tenants.Commands.CreateTenant;
using AkyildizYonetim.Application.Tenants.Commands.DeleteTenant;
using AkyildizYonetim.Application.Tenants.Commands.UpdateTenant;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantById;
using AkyildizYonetim.Application.Tenants.Queries.GetTenants;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantStats;
using System;

using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<IActionResult> GetTenants([FromQuery] bool? isActive, [FromQuery] string? searchTerm, [FromQuery] DateTime? period, [FromQuery] bool? showOnlyOccupied, [FromQuery] int? floor, [FromQuery] string? category)
    {
        try
        {
            var result = await _mediator.Send(new GetTenantsQuery { IsActive = isActive, SearchTerm = searchTerm, Period = period, ShowOnlyOccupied = showOnlyOccupied, Floor = floor, Category = category });
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetTenantStats()
    {
        try
        {
            var result = await _mediator.Send(new GetTenantStatsQuery());
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetTenantByIdQuery { Id = id });
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        try
        {
            // Validate request
            if (request == null)
                return BadRequest("Request body is required");
                
            if (string.IsNullOrEmpty(request.Name))
                return BadRequest("Name is required");
                
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required");
                
            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest("Phone is required");
                
            if (string.IsNullOrEmpty(request.IdentityNumber))
                return BadRequest("Identity number is required");
                
            if (request.FlatId == Guid.Empty)
                return BadRequest("Flat ID is required");
                
            if (request.MonthlyRent <= 0)
                return BadRequest("Monthly rent must be greater than 0");
            
            var command = new CreateTenantCommand
            {
                CompanyName = request.Name,
                BusinessType = "Ticaret", // Default value, can be made configurable
                TaxNumber = request.IdentityNumber,
                ContactPersonName = request.Name,
                ContactPersonPhone = request.Phone,
                ContactPersonEmail = request.Email,
                FlatId = request.FlatId, // Now it's already a Guid
                MonthlyAidat = request.MonthlyRent,
                ElectricityRate = 1.50m, // Default value
                WaterRate = 8.00m, // Default value
                ContractStartDate = !string.IsNullOrEmpty(request.RentStartDate) ? DateTime.Parse(request.RentStartDate) : null,
                ContractEndDate = !string.IsNullOrEmpty(request.RentEndDate) ? DateTime.Parse(request.RentEndDate) : null
            };
            
            var result = await _mediator.Send(command);
            return result.IsSuccess ? CreatedAtAction(nameof(GetTenantById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        try
        {
            var command = new UpdateTenantCommand
            {
                Id = id,
                CompanyName = request.Name,
                BusinessType = "Ticaret", // Default value, can be made configurable
                TaxNumber = request.IdentityNumber,
                ContactPersonName = request.Name,
                ContactPersonPhone = request.Phone,
                ContactPersonEmail = request.Email,
                MonthlyAidat = request.MonthlyRent,
                ElectricityRate = 1.50m, // Default value
                WaterRate = 8.00m, // Default value
                ContractStartDate = !string.IsNullOrEmpty(request.RentStartDate) ? DateTime.Parse(request.RentStartDate) : null,
                ContractEndDate = !string.IsNullOrEmpty(request.RentEndDate) ? DateTime.Parse(request.RentEndDate) : null,
                IsActive = true
            };
            
            var result = await _mediator.Send(command);
            return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteTenantCommand { Id = id });
            return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpGet("available-flats")]
    public async Task<IActionResult> GetAvailableFlats([FromQuery] int? floor, [FromQuery] string? category, [FromQuery] string? searchTerm)
    {
        try
        {
            var result = await _mediator.Send(new GetAvailableFlatsQuery { Floor = floor, Category = category, SearchTerm = searchTerm });
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message, details = ex.StackTrace });
        }
    }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public Guid FlatId { get; set; } // Changed from int to Guid
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
    public Guid FlatId { get; set; } // Changed from int to Guid
    public string RentStartDate { get; set; } = string.Empty;
    public string RentEndDate { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public decimal Deposit { get; set; }
} 