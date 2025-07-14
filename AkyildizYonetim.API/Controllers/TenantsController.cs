using AkyildizYonetim.Application.Tenants.Commands.CreateTenant;
using AkyildizYonetim.Application.Tenants.Commands.DeleteTenant;
using AkyildizYonetim.Application.Tenants.Commands.UpdateTenant;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantById;
using AkyildizYonetim.Application.Tenants.Queries.GetTenants;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantStats;
using System;
using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// Yeni bir kiracı oluşturur
    /// </summary>
    /// <param name="request">Kiracı bilgileri</param>
    /// <returns>Oluşturulan kiracının ID'si</returns>
    /// <response code="201">Kiracı başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz veri veya validasyon hatası</response>
    /// <response code="500">Sunucu hatası</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        try
        {
            // Log incoming request for debugging
            var logger = HttpContext.RequestServices.GetService<ILogger<TenantsController>>();
            logger?.LogInformation("CreateTenant called with request: {@Request}", request);
            
            // Validate request
            if (request == null)
                return BadRequest(new { error = "Request body is required" });
                
            if (string.IsNullOrEmpty(request.Name))
                return BadRequest(new { error = "Name is required" });
                
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { error = "Email is required" });
                
            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest(new { error = "Phone is required" });
                
            if (string.IsNullOrEmpty(request.IdentityNumber))
                return BadRequest(new { error = "Identity number is required" });
                
            if (!request.FlatId.HasValue || request.FlatId.Value == Guid.Empty)
                return BadRequest(new { error = "Flat ID is required" });
                
            if (request.MonthlyRent <= 0)
                return BadRequest(new { error = "Monthly rent must be greater than 0" });
            
            var command = new CreateTenantCommand
            {
                CompanyName = !string.IsNullOrEmpty(request.CompanyName) ? request.CompanyName : request.Name,
                BusinessType = !string.IsNullOrEmpty(request.BusinessType) ? request.BusinessType : "Ticaret",
                TaxNumber = request.IdentityNumber,
                ContactPersonName = !string.IsNullOrEmpty(request.ContactPersonName) ? request.ContactPersonName : request.Name,
                ContactPersonPhone = !string.IsNullOrEmpty(request.ContactPersonPhone) ? request.ContactPersonPhone : request.Phone,
                ContactPersonEmail = !string.IsNullOrEmpty(request.ContactPersonEmail) ? request.ContactPersonEmail : request.Email,
                FlatId = request.FlatId.Value,
                MonthlyAidat = request.MonthlyRent,
                ElectricityRate = 1.50m, // Default value
                WaterRate = 8.00m, // Default value
                ContractStartDate = !string.IsNullOrEmpty(request.RentStartDate) ? DateTime.Parse(request.RentStartDate) : null,
                ContractEndDate = !string.IsNullOrEmpty(request.RentEndDate) ? DateTime.Parse(request.RentEndDate) : null
            };
            
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                logger?.LogInformation("Tenant created successfully with ID: {TenantId}", result.Data);
                return CreatedAtAction(nameof(GetTenantById), new { id = result.Data }, new { id = result.Data, message = "Tenant created successfully" });
            }
            else
            {
                logger?.LogWarning("Failed to create tenant: {Errors}", result.Errors);
                return BadRequest(new { 
                    error = "Failed to create tenant", 
                    message = result.ErrorMessage ?? string.Join(", ", result.Errors),
                    details = result.Errors
                });
            }
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
    
    /// <summary>
    /// Test endpoint - gelen veriyi loglar
    /// </summary>
    [HttpPost("test")]
    public IActionResult TestRequest([FromBody] object request)
    {
        var logger = HttpContext.RequestServices.GetService<ILogger<TenantsController>>();
        logger?.LogInformation("Test request received: {@Request}", request);
        
        return Ok(new { 
            message = "Test request received", 
            receivedData = request,
            timestamp = DateTime.UtcNow
        });
    }
}

public class CreateTenantRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20, ErrorMessage = "Phone cannot be longer than 20 characters")]
    public string Phone { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Identity number is required")]
    [StringLength(20, ErrorMessage = "Identity number cannot be longer than 20 characters")]
    public string IdentityNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Flat ID is required")]
    public Guid? FlatId { get; set; }
    
    public string? RentStartDate { get; set; }
    public string? RentEndDate { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be greater than 0")]
    public decimal MonthlyRent { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Deposit cannot be negative")]
    public decimal Deposit { get; set; }
    
    // Additional fields that might be sent from frontend
    [StringLength(100, ErrorMessage = "Company name cannot be longer than 100 characters")]
    public string? CompanyName { get; set; }
    
    [StringLength(50, ErrorMessage = "Business type cannot be longer than 50 characters")]
    public string? BusinessType { get; set; }
    
    [StringLength(100, ErrorMessage = "Contact person name cannot be longer than 100 characters")]
    public string? ContactPersonName { get; set; }
    
    [StringLength(20, ErrorMessage = "Contact person phone cannot be longer than 20 characters")]
    public string? ContactPersonPhone { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid contact person email format")]
    public string? ContactPersonEmail { get; set; }
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