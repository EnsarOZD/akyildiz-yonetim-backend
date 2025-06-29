using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Payments.Queries.GetPaymentById;

public record GetPaymentByIdQuery : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; init; }
}

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Where(p => p.Id == request.Id && !p.IsDeleted)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Type = p.Type,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                Description = p.Description,
                ReceiptNumber = p.ReceiptNumber,
                OwnerId = p.OwnerId,
                TenantId = p.TenantId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            return Result<PaymentDto>.Failure("Ödeme bulunamadı.");

        return Result<PaymentDto>.Success(payment);
    }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 