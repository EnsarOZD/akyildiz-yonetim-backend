using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
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

        // 1. Önce bu ödemelere ait AdvanceUsage kayıtlarını bul ve sil
        var advanceUsages = await _context.AdvanceUsages
            .Where(x => paymentIds.Contains(x.PaymentId))
            .ToListAsync(cancellationToken);

        if (advanceUsages.Any())
        {
            _context.AdvanceUsages.RemoveRange(advanceUsages);
        }

        // 2. Ardından bu ödemelere ait DebtAllocation kayıtlarını bul
        var allocations = await _context.DebtAllocations
            .Where(x => paymentIds.Contains(x.PaymentId))
            .Include(a => a.UtilityDebt)
            .ToListAsync(cancellationToken);

        if (allocations.Any())
        {
            // İlgili borçların kalan tutarlarını geri yükle
            foreach (var allocation in allocations)
            {
                if (allocation.UtilityDebt != null)
                {
                    allocation.UtilityDebt.RemainingAmount += allocation.Amount;
                    allocation.UtilityDebt.Status = allocation.UtilityDebt.RemainingAmount >= allocation.UtilityDebt.Amount 
                        ? Domain.Enums.DebtStatus.Unpaid 
                        : Domain.Enums.DebtStatus.Partial;
                }
            }
            _context.DebtAllocations.RemoveRange(allocations);
        }

        // Son olarak ödemeleri sil
        _context.Payments.RemoveRange(payments);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
