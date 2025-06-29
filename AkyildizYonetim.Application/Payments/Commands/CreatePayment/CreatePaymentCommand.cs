using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand : IRequest<Result<Guid>>
{
    public decimal Amount { get; init; }
    public PaymentType Type { get; init; }
    public PaymentStatus Status { get; init; }
    public DateTime PaymentDate { get; init; }
    public string? Description { get; init; }
    public string? ReceiptNumber { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreatePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Type = request.Type,
            Status = request.Status,
            PaymentDate = request.PaymentDate,
            Description = request.Description,
            ReceiptNumber = request.ReceiptNumber,
            OwnerId = request.OwnerId,
            TenantId = request.TenantId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(payment.Id);
    }
} 