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
            logger?.LogInformation("FlatId: {FlatId}, FlatIdInt: {FlatIdInt}", request.FlatId, request.FlatIdInt);
            logger?.LogInformation("RentStartDate: {RentStartDate}, RentEndDate: {RentEndDate}", request.RentStartDate, request.RentEndDate);
            
            // Flat ID validation and conversion
            Guid? flatId = null;
            if (request.FlatId.HasValue && request.FlatId.Value != Guid.Empty)
            {
                flatId = request.FlatId.Value;
            }
            else if (request.FlatIdInt.HasValue)
            {
                // Try to find flat by number or other identifier
                // This is a placeholder - you might need to adjust based on your flat numbering system
                logger?.LogWarning("FlatIdInt provided: {FlatIdInt}, but conversion to Guid not implemented", request.FlatIdInt.Value);
                return BadRequest(new { error = "Flat ID conversion not supported. Please provide FlatId as Guid." });
            }
            
            if (!flatId.HasValue)
                return BadRequest(new { error = "Flat ID is required" });
            
            // Date parsing
            DateTime? contractStartDate = null;
            if (request.RentStartDateDt.HasValue)
            {
                contractStartDate = request.RentStartDateDt.Value;
            }
            else if (!string.IsNullOrEmpty(request.RentStartDate))
            {
                if (DateTime.TryParse(request.RentStartDate, out var startDate))
                    contractStartDate = startDate;
            }
            
            DateTime? contractEndDate = null;
            if (request.RentEndDateDt.HasValue)
            {
                contractEndDate = request.RentEndDateDt.Value;
            }
            else if (!string.IsNullOrEmpty(request.RentEndDate))
            {
                if (DateTime.TryParse(request.RentEndDate, out var endDate))
                    contractEndDate = endDate;
            }
            
            var command = new CreateTenantCommand
            {
                CompanyName = !string.IsNullOrEmpty(request.CompanyName) ? request.CompanyName : request.Name,
                BusinessType = !string.IsNullOrEmpty(request.BusinessType) ? request.BusinessType : "Ticaret",
                TaxNumber = request.IdentityNumber,
                ContactPersonName = !string.IsNullOrEmpty(request.ContactPersonName) ? request.ContactPersonName : request.Name,
                ContactPersonPhone = !string.IsNullOrEmpty(request.ContactPersonPhone) ? request.ContactPersonPhone : request.Phone,
                ContactPersonEmail = !string.IsNullOrEmpty(request.ContactPersonEmail) ? request.ContactPersonEmail : request.Email,
                FlatId = flatId.Value,
                MonthlyAidat = request.MonthlyRent,
                ElectricityRate = 1.50m, // Default value
                WaterRate = 8.00m, // Default value
                ContractStartDate = contractStartDate,
                ContractEndDate = contractEndDate
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
                ContractStartDate = !string.IsNullOrEmpty(request.RentStartDate) ? 
                    (DateTime.TryParse(request.RentStartDate, out var startDate) ? startDate : null) : null,
                ContractEndDate = !string.IsNullOrEmpty(request.RentEndDate) ? 
                    (DateTime.TryParse(request.RentEndDate, out var endDate) ? endDate : null) : null,
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
    /// Test endpoint - gelen veriyi detaylı loglar
    /// </summary>
    [HttpPost("test")]
    public IActionResult TestRequest([FromBody] object request)
    {
        var logger = HttpContext.RequestServices.GetService<ILogger<TenantsController>>();
        logger?.LogInformation("Test request received: {@Request}", request);
        
        // Try to parse as CreateTenantRequest to see what fields are present
        try
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(request);
            logger?.LogInformation("Request as JSON: {JsonString}", jsonString);
            
            // Try to deserialize to see what we get
            var tenantRequest = System.Text.Json.JsonSerializer.Deserialize<CreateTenantRequest>(jsonString);
            logger?.LogInformation("Parsed as CreateTenantRequest: {@TenantRequest}", tenantRequest);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Could not parse request as CreateTenantRequest: {Error}", ex.Message);
        }
        
        return Ok(new { 
            message = "Test request received and logged", 
            receivedData = request,
            timestamp = DateTime.UtcNow,
            note = "Check logs for detailed information"
        });
    }
}

public class CreateTenantRequest
{
    // Basic fields (required)
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
    
    // Flat ID - can be Guid or int from frontend
    public Guid? FlatId { get; set; }
    public int? FlatIdInt { get; set; } // Alternative for int format
    
    // Date fields - can be string or DateTime
    public string? RentStartDate { get; set; }
    public string? RentEndDate { get; set; }
    public DateTime? RentStartDateDt { get; set; }
    public DateTime? RentEndDateDt { get; set; }
    
    // Financial fields
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