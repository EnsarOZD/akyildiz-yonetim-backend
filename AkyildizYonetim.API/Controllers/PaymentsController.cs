using AkyildizYonetim.Application.Payments.Commands.CreatePayment;
using AkyildizYonetim.Application.Payments.Commands.DeletePayment;
using AkyildizYonetim.Application.Payments.Commands.UpdatePayment;
using AkyildizYonetim.Application.Payments.Queries.GetPaymentById;
using AkyildizYonetim.Application.Payments.Queries.GetPayments;
using AkyildizYonetim.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using AkyildizYonetim.Domain.Entities;


namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ödemeleri listeler
    /// </summary>
    /// <param name="type">Ödeme türü</param>
    /// <param name="status">Ödeme durumu</param>
    /// <param name="ownerId">Mal sahibi ID</param>
    /// <param name="tenantId">Kiracı ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Ödeme listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] PaymentType? type,
        [FromQuery] PaymentStatus? status,
        [FromQuery] string? ownerId,
        [FromQuery] string? tenantId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool excludeAdvanceUse = false)
    {
        Guid? ownerGuid = null;
        if (Guid.TryParse(ownerId, out var parsedOwnerId)) ownerGuid = parsedOwnerId;

        Guid? tenantGuid = null;
        if (Guid.TryParse(tenantId, out var parsedTenantId)) tenantGuid = parsedTenantId;

        var result = await _mediator.Send(new GetPaymentsQuery
        {
            Type = type,
            Status = status,
            OwnerId = ownerGuid,
            TenantId = tenantGuid,
            StartDate = startDate,
            EndDate = endDate,
            ExcludeAdvanceUse = excludeAdvanceUse
        });

        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    /// <summary>
    /// ID'ye göre ödeme getirir
    /// </summary>
    /// <param name="id">Ödeme ID</param>
    /// <returns>Ödeme bilgileri</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), 200)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery { Id = id });
        return result.IsSuccess 
            ? Ok(result.Data) 
            : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Yeni bir ödeme oluşturur
    /// </summary>
    /// <param name="request">Ödeme bilgileri</param>
    /// <returns>Oluşturulan ödeme</returns>
    [Authorize(Policy = "TenantWrite")]
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required" });

        if (!DateTime.TryParse(request.PaymentDate, out var paymentDate))
            return BadRequest(new { error = "Invalid payment date format" });

        var command = new CreatePaymentCommand
        {
            Amount = request.Amount,
            Type = request.Type ?? PaymentType.Other,
            Status = request.Status ?? PaymentStatus.Completed,
            PaymentDate = paymentDate,
            Description = request.Description,
            ReceiptNumber = request.ReceiptNumber,
            TenantId = request.TenantId,
            OwnerId = request.OwnerId
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPaymentById), new { id = result.Data.Id }, result.Data)
            : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("with-allocation")]
    public async Task<IActionResult> CreatePaymentWithAllocation([FromBody] CreatePaymentWithDebtAllocationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPaymentById), new { id = result.Data.Payment.Id }, result.Data)
            : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Ödeme siler
    /// </summary>
    /// <param name="id">Ödeme ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    [Authorize(Policy = "TenantWrite")]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> DeletePayment(Guid id)
    {
        var result = await _mediator.Send(new DeletePaymentCommand { Id = id });
        return result.IsSuccess 
            ? NoContent() 
            : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
    
    /// <summary>
    /// Test endpoint - gelen veriyi loglar
    /// </summary>
    [HttpPost("test")]
    public IActionResult TestRequest([FromBody] object request)
    {
        var logger = HttpContext.RequestServices.GetService<ILogger<PaymentsController>>();
        logger?.LogInformation("Test request received: {@Request}", request);
        
        return Ok(new { 
            message = "Test request received", 
            receivedData = request,
            timestamp = DateTime.UtcNow
        });
    }
}

public class CreatePaymentRequest
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Payment date is required")]
    public string PaymentDate { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "Payment method cannot be longer than 50 characters")]
    public string PaymentMethod { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "Receipt number cannot be longer than 50 characters")]
    public string ReceiptNumber { get; set; } = string.Empty;
    
    // Tenant or Owner ID (one of them should be provided)
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    
    // Payment type and status
    public PaymentType? Type { get; set; }
    public PaymentStatus? Status { get; set; }
}

public class UpdatePaymentRequest
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Payment date is required")]
    public string PaymentDate { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "Payment method cannot be longer than 50 characters")]
    public string PaymentMethod { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "Receipt number cannot be longer than 50 characters")]
    public string ReceiptNumber { get; set; } = string.Empty;
    
    // Tenant or Owner ID (one of them should be provided)
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    
    // Payment type and status
    public PaymentType? Type { get; set; }
    public PaymentStatus? Status { get; set; }
} 