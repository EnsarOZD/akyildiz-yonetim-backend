using AkyildizYonetim.Application.Payments.Commands.CreatePayment;
using AkyildizYonetim.Application.Payments.Commands.DeletePayment;
using AkyildizYonetim.Application.Payments.Commands.UpdatePayment;
using AkyildizYonetim.Application.Payments.Queries.GetPaymentById;
using AkyildizYonetim.Application.Payments.Queries.GetPayments;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments(
        [FromQuery] PaymentType? type,
        [FromQuery] PaymentStatus? status,
        [FromQuery] Guid? ownerId,
        [FromQuery] Guid? tenantId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _mediator.Send(new GetPaymentsQuery
        {
            Type = type,
            Status = status,
            OwnerId = ownerId,
            TenantId = tenantId,
            StartDate = startDate,
            EndDate = endDate
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetPaymentById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(Guid id)
    {
        var result = await _mediator.Send(new DeletePaymentCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
}

public class CreatePaymentRequest
{
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
}

public class UpdatePaymentRequest
{
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
} 