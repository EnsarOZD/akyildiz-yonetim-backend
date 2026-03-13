using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;

public record UseAdvanceAccountCommand : IRequest<Result<AdvanceAccountUsageDto>>
{
    public Guid TenantId { get; init; }
    public List<DebtPaymentRequest> DebtPayments { get; init; } = new();
    public string? Description { get; init; }
}

public class UseAdvanceAccountCommandHandler 
    : IRequestHandler<UseAdvanceAccountCommand, Result<AdvanceAccountUsageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UseAdvanceAccountCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UseAdvanceAccountCommandHandler(
        IApplicationDbContext context,
        ILogger<UseAdvanceAccountCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AdvanceAccountUsageDto>> Handle(
        UseAdvanceAccountCommand request, 
        CancellationToken cancellationToken)
    {
        // Internal roles (admin, manager, dataentry) can operate. 
        // Observers are read-only. External roles (Tenants) resolved via claims.
        var effectiveTenantId = request.TenantId;

        if (!_currentUserService.IsAdmin && !_currentUserService.IsManager && !_currentUserService.IsDataEntry)
        {
            // If external tenant, override with their own identity
            if (_currentUserService.TenantId.HasValue)
            {
                effectiveTenantId = _currentUserService.TenantId.Value;
            }
            else
            {
                return Result<AdvanceAccountUsageDto>.Failure("Bu işlemi yapmaya yetkiniz yok.");
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Avans hesabını kontrol et
            var advanceAccount = await _context.AdvanceAccounts
                .FirstOrDefaultAsync(aa => aa.TenantId == effectiveTenantId && !aa.IsDeleted, cancellationToken);

            if (advanceAccount == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<AdvanceAccountUsageDto>.Failure("Avans hesabı bulunamadı.");
            }

            var totalRequestedAmount = request.DebtPayments.Sum(dp => dp.Amount);
            
            if (advanceAccount.Balance < totalRequestedAmount)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<AdvanceAccountUsageDto>.Failure(
                    $"Yetersiz bakiye. Mevcut: {advanceAccount.Balance:C}, İstenen: {totalRequestedAmount:C}");
            }

            // 2. Sanal ödeme oluştur (avans kullanımı için)
            var virtualPayment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = totalRequestedAmount,
                Type = PaymentType.Utility, // Avans kullanımı
                Status = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow,
                Description = request.Description ?? "Avans hesabından borç ödeme",
                ReceiptNumber = $"AVANS-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                TenantId = effectiveTenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(virtualPayment);

            var paymentResults = new List<DebtPaymentResult>();
            decimal remainingAdvanceBalance = advanceAccount.Balance;

            // 3. Her borç için ödeme yap
            foreach (var debtPayment in request.DebtPayments)
            {
                var debt = await _context.UtilityDebts
                    .FirstOrDefaultAsync(d => d.Id == debtPayment.DebtId && !d.IsDeleted, cancellationToken);

                if (debt == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<AdvanceAccountUsageDto>.Failure($"Borç bulunamadı: {debtPayment.DebtId}");
                }

                if (debt.TenantId != effectiveTenantId)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<AdvanceAccountUsageDto>.Failure($"Borç bu kiracıya ait değil: {debtPayment.DebtId}");
                }

                var paymentAmount = Math.Min(debtPayment.Amount, debt.RemainingAmount);
                
                if (paymentAmount > 0)
                {
                    // PaymentDebt kaydı oluştur
                    var paymentDebt = new PaymentDebt
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = virtualPayment.Id,
                        DebtId = debt.Id,
                        PaidAmount = paymentAmount,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PaymentDebts.Add(paymentDebt);

                    // Borç durumunu güncelle
                    debt.PaidAmount = (debt.PaidAmount ?? 0) + paymentAmount;
                    debt.RemainingAmount = debt.Amount - debt.PaidAmount.Value;
                    debt.Status = debt.RemainingAmount <= 0 ? DebtStatus.Paid : DebtStatus.Partial;
                    debt.UpdatedAt = DateTime.UtcNow;

                    paymentResults.Add(new DebtPaymentResult
                    {
                        DebtId = debt.Id,
                        DebtDescription = debt.Description ?? $"{debt.Type} - {debt.PeriodYear}/{debt.PeriodMonth}",
                        PaidAmount = paymentAmount,
                        RemainingDebtAmount = debt.RemainingAmount,
                        DebtStatus = debt.Status
                    });
                }
            }

            // 4. Avans hesabı bakiyesini güncelle
            advanceAccount.Balance -= totalRequestedAmount;
            advanceAccount.UpdatedAt = DateTime.UtcNow;

            // 5. Değişiklikleri kaydet
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Avans hesabı kullanıldı: TenantId={TenantId}, Amount={Amount}, Balance={Balance}", 
                request.TenantId, totalRequestedAmount, advanceAccount.Balance);

            var result = new AdvanceAccountUsageDto
            {
                PaymentId = virtualPayment.Id,
                TenantId = effectiveTenantId,
                TotalAmount = totalRequestedAmount,
                PreviousBalance = advanceAccount.Balance + totalRequestedAmount,
                NewBalance = advanceAccount.Balance,
                DebtPayments = paymentResults,
                Description = virtualPayment.Description,
                PaymentDate = virtualPayment.PaymentDate
            };

            return Result<AdvanceAccountUsageDto>.Success(result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Avans hesabı kullanılırken hata oluştu: TenantId={TenantId}", request.TenantId);
            return Result<AdvanceAccountUsageDto>.Failure($"Avans hesabı kullanılamadı: {ex.Message}");
        }
    }
}

 