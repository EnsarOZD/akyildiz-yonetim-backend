using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.DeleteUtilityDebt;

public record DeleteUtilityDebtCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteUtilityDebtCommandHandler : IRequestHandler<DeleteUtilityDebtCommand, Result>
{
    private readonly IApplicationDbContext _context;    
    public DeleteUtilityDebtCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result> Handle(DeleteUtilityDebtCommand request, CancellationToken cancellationToken)
    {
        var debt = await _context.UtilityDebts
            .Include(d => d.PaymentDebts)
                .ThenInclude(pd => pd.Payment)
            .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);

        if (debt == null)
            return Result.Failure("Borç kaydı bulunamadı.");

        // 1. Avans iadesi kontrolü (Eğer avans ile ödendiyse)
        foreach (var pd in debt.PaymentDebts.Where(x => !x.IsDeleted))
        {
            if (pd.Payment.ReceiptNumber?.StartsWith("AVANS-") == true && debt.TenantId.HasValue)
            {
                var advanceAccount = await _context.AdvanceAccounts
                    .FirstOrDefaultAsync(aa => aa.TenantId == debt.TenantId.Value && !aa.IsDeleted, cancellationToken);
                
                if (advanceAccount != null)
                {
                    advanceAccount.Balance += pd.PaidAmount;
                    advanceAccount.UpdatedAt = DateTime.UtcNow;
                }
            }
            
            // PaymentDebt kaydını da sil
            pd.IsDeleted = true;
            pd.UpdatedAt = DateTime.UtcNow;
        }

        debt.IsDeleted = true;
        debt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}