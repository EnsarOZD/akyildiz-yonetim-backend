using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.RecalculateBalances;

public record RecalculateAdvanceBalancesCommand : IRequest<Result<RecalculateResultDto>>;

public class RecalculateResultDto
{
    public int TotalAccountsProcessed { get; set; }
    public int UpdatedAccountsCount { get; set; }
    public List<BalanceUpdateDto> Updates { get; set; } = new();
}

public class BalanceUpdateDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public decimal Adjustment { get; set; }
}

public class RecalculateAdvanceBalancesCommandHandler : IRequestHandler<RecalculateAdvanceBalancesCommand, Result<RecalculateResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RecalculateAdvanceBalancesCommandHandler> _logger;

    public RecalculateAdvanceBalancesCommandHandler(IApplicationDbContext context, ILogger<RecalculateAdvanceBalancesCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<RecalculateResultDto>> Handle(RecalculateAdvanceBalancesCommand request, CancellationToken cancellationToken)
    {
        var result = new RecalculateResultDto();
        
        // 1. Tüm kiracıları (veya sadece avans hesabı olanları) alalım
        // Veri tutarlılığı için tüm kiracıları kontrol etmek daha güvenli (hesabı olmayan ama bakiyesi birikenler olabilir)
        var tenants = await _context.Tenants
            .Include(t => t.AdvanceAccount)
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            result.TotalAccountsProcessed++;

            // 1. Bu kiracıya ait tüm GERÇEK tahsilatları topla (AVANS kullanımı hariç)
            var totalPayments = await _context.Payments
                .Where(p => p.TenantId == tenant.Id && !p.IsDeleted && !(p.ReceiptNumber != null && p.ReceiptNumber.StartsWith("AVANS-")))
                .SumAsync(p => p.Amount, cancellationToken);

            // 2. Bu kiracıya ait tüm BORÇ EŞLEŞTİRMELERİ (dağıtılan tutarları) topla
            // Kriter: Ödeme silinmemiş OLMALI, PaymentDebt silinmemiş OLMALI
            var totalAllocated = await _context.PaymentDebts
                .Include(pd => pd.Payment)
                .Where(pd => pd.Payment.TenantId == tenant.Id && !pd.IsDeleted && !pd.Payment.IsDeleted)
                .SumAsync(pd => pd.PaidAmount, cancellationToken);

            // Olması gereken bakiye = Toplam Nakit Girişi - Borçlara Dağıtılan Toplam Tutar
            var calculatedBalance = totalPayments - totalAllocated;
            if (calculatedBalance < 0) calculatedBalance = 0; // Negatif bakiye mantıken olmamalı

            var currentBalance = tenant.AdvanceAccount?.Balance ?? 0;

            if (Math.Abs(currentBalance - calculatedBalance) > 0.001m)
            {
                if (tenant.AdvanceAccount == null)
                {
                    tenant.AdvanceAccount = new AdvanceAccount
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenant.Id,
                        Balance = calculatedBalance,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.AdvanceAccounts.Add(tenant.AdvanceAccount);
                }
                else
                {
                    tenant.AdvanceAccount.Balance = calculatedBalance;
                    tenant.AdvanceAccount.UpdatedAt = DateTime.UtcNow;
                }

                result.UpdatedAccountsCount++;
                result.Updates.Add(new BalanceUpdateDto
                {
                    TenantId = tenant.Id,
                    TenantName = tenant.CompanyName ?? tenant.ContactPersonName,
                    PreviousBalance = currentBalance,
                    NewBalance = calculatedBalance,
                    Adjustment = calculatedBalance - currentBalance
                });
            }
        }

        if (result.UpdatedAccountsCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Advance balances recalculated. Updated {Count} accounts.", result.UpdatedAccountsCount);
        }

        return Result<RecalculateResultDto>.Success(result);
    }
}
