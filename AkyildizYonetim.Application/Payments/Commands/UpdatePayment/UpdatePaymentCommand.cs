using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Payments.Commands.UpdatePayment;

public record UpdatePaymentCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public PaymentType Type { get; init; }
    public PaymentStatus Status { get; init; }
    public DateTime PaymentDate { get; init; }
    public string? Description { get; init; }
    public string? ReceiptNumber { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
}

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdatePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (payment == null)
            return Result.Failure("Ödeme bulunamadı.");

        payment.Amount = request.Amount;
        payment.Type = request.Type;
        payment.Status = request.Status;
        payment.PaymentDate = request.PaymentDate;
        payment.Description = request.Description;
        payment.ReceiptNumber = request.ReceiptNumber;
        payment.OwnerId = request.OwnerId;
        payment.TenantId = request.TenantId;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 