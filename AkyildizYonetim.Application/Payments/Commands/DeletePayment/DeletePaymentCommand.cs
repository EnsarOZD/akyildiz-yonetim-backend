using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Payments.Commands.DeletePayment;

public record DeletePaymentCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeletePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (payment == null)
            return Result.Failure("Ödeme bulunamadı.");

        payment.IsDeleted = true;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 