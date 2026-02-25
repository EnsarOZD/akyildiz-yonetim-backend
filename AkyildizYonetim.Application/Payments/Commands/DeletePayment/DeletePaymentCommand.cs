using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
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
            .Include(p => p.PaymentDebts)
                .ThenInclude(pd => pd.Debt)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (payment == null)
            return Result.Failure("Ödeme bulunamadı.");

        // 1. Borç durumlarını geri al ve silinen ödenen miktarları topla
        decimal allocatedAmount = 0;
        foreach (var pd in payment.PaymentDebts.Where(x => !x.IsDeleted))
        {
            var debt = pd.Debt;
            debt.PaidAmount = (debt.PaidAmount ?? 0) - pd.PaidAmount;
            debt.RemainingAmount = debt.Amount - (debt.PaidAmount ?? 0);
            debt.Status = debt.RemainingAmount <= 0 ? DebtStatus.Paid : (debt.PaidAmount > 0 ? DebtStatus.Partial : DebtStatus.Unpaid);
            debt.UpdatedAt = DateTime.UtcNow;

            allocatedAmount += pd.PaidAmount;

            pd.IsDeleted = true;
            pd.UpdatedAt = DateTime.UtcNow;
        }

        // 2. Avans Düzenlemeleri
        if (payment.TenantId.HasValue)
        {
            var advanceAccount = await _context.AdvanceAccounts
                .FirstOrDefaultAsync(aa => aa.TenantId == payment.TenantId.Value && !aa.IsDeleted, cancellationToken);

            if (advanceAccount != null)
            {
                // A) Eğer bu ödeme avans hesabından YAPILDIYSA (bakiyeyi azaltmıştı), bakiyeyi GERİ YÜKLE
                if (payment.ReceiptNumber?.StartsWith("AVANS-") == true)
                {
                    advanceAccount.Balance += payment.Amount;
                }
                else
                {
                    // B) Eğer bu normal bir ödeme ise ve bir kısmı AVANS HESABINA AKTARILDIYSA (surplus), o kısmı bakiyeden DÜŞ
                    var surplus = payment.Amount - allocatedAmount;
                    if (surplus > 0)
                    {
                        advanceAccount.Balance -= surplus;
                    }
                }
                advanceAccount.UpdatedAt = DateTime.UtcNow;
            }
        }

        payment.IsDeleted = true;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}