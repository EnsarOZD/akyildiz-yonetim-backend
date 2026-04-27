using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Payments.Commands.DeleteBulkPayments;

public record DeleteBulkPaymentsCommand : IRequest<Result>
{
    public List<Guid> Ids { get; init; } = new();
}

public class DeleteBulkPaymentsCommandHandler : IRequestHandler<DeleteBulkPaymentsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteBulkPaymentsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBulkPaymentsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || !request.Ids.Any())
            return Result.Failure("Silinecek ödeme kaydı bulunamadı.");

        var payments = await _context.Payments
            .Where(p => request.Ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (!payments.Any())
            return Result.Failure("Belirtilen ödemeler bulunamadı.");

        var paymentIds = payments.Select(x => x.Id).ToList();

        // Find PaymentDebt junction records and restore debt remaining amounts
        var paymentDebts = await _context.PaymentDebts
            .Where(pd => paymentIds.Contains(pd.PaymentId))
            .Include(pd => pd.Debt)
            .ToListAsync(cancellationToken);

        foreach (var pd in paymentDebts)
        {
            if (pd.Debt != null)
            {
                pd.Debt.PaidAmount = Math.Max(0, (pd.Debt.PaidAmount ?? 0) - pd.PaidAmount);
                pd.Debt.RemainingAmount = pd.Debt.Amount - pd.Debt.PaidAmount.Value;
                pd.Debt.Status = pd.Debt.RemainingAmount <= 0
                    ? DebtStatus.Paid
                    : (pd.Debt.PaidAmount > 0 ? DebtStatus.Partial : DebtStatus.Unpaid);
                pd.Debt.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.PaymentDebts.RemoveRange(paymentDebts);
        _context.Payments.RemoveRange(payments);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
