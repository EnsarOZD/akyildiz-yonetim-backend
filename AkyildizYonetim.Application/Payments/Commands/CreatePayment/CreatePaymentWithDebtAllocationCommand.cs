using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Application.Payments.Queries.GetPaymentById;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Payments.Commands.CreatePayment;

public record CreatePaymentWithDebtAllocationCommand : IRequest<Result<PaymentWithAllocationDto>>
{
    public decimal Amount { get; init; }
    public PaymentType Type { get; init; }
    public DateTime PaymentDate { get; init; }
    public string? Description { get; init; }
    public string? ReceiptNumber { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
    
    // Borç eşleştirme seçenekleri
    public List<DebtAllocationRequest>? DebtAllocations { get; init; } // Manuel eşleştirme
    public bool AutoAllocate { get; init; } = true; // Otomatik eşleştirme
    public bool UseAdvanceAccount { get; init; } = false; // Avans hesabı kullan
}

public record DebtAllocationRequest
{
    public Guid DebtId { get; init; }
    public decimal Amount { get; init; }
}

public class CreatePaymentWithDebtAllocationCommandHandler 
    : IRequestHandler<CreatePaymentWithDebtAllocationCommand, Result<PaymentWithAllocationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreatePaymentWithDebtAllocationCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreatePaymentWithDebtAllocationCommandHandler(
        IApplicationDbContext context, 
        ILogger<CreatePaymentWithDebtAllocationCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaymentWithAllocationDto>> Handle(
        CreatePaymentWithDebtAllocationCommand request, 
        CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Data Scope Resolution
            var fullAccessRoles = new Func<ICurrentUserService, bool>[] { 
                u => u.IsAdmin, u => u.IsManager, u => u.IsDataEntry 
            };

            var effectiveTenantId = DataScopeHelper.ResolveTenantId(_currentUserService, request.TenantId, fullAccessRoles);
            var effectiveOwnerId = DataScopeHelper.ResolveOwnerId(_currentUserService, request.OwnerId, fullAccessRoles);

            if (DataScopeHelper.IsScopeRestricted(_currentUserService, fullAccessRoles))
            {
                // No need to check for empty result here as this is a command, 
                // but we should ensure the request is not spoofing.
                // The Resolve methods already handle the claim override.
            }

            // 1. Ödeme oluştur
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Type = request.Type,
                Status = PaymentStatus.Completed, // Otomatik olarak tamamlandı
                PaymentDate = request.PaymentDate,
                Description = request.Description,
                ReceiptNumber = request.ReceiptNumber,
                OwnerId = effectiveOwnerId,
                TenantId = effectiveTenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            var allocationResults = new List<DebtAllocationResult>();
            decimal remainingAmount = request.Amount;

            // 2. Manuel borç eşleştirme (varsa)
            if (request.DebtAllocations?.Any() == true)
            {
                foreach (var allocation in request.DebtAllocations)
                {
                    if (remainingAmount <= 0) break;

                    var debt = await _context.UtilityDebts
                        .FirstOrDefaultAsync(d => d.Id == allocation.DebtId && !d.IsDeleted, cancellationToken);

                    if (debt == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<PaymentWithAllocationDto>.Failure($"Borç bulunamadı: {allocation.DebtId}");
                    }

                    var allocationAmount = Math.Min(allocation.Amount, remainingAmount);
                    var allocationAmount2 = Math.Min(allocationAmount, debt.RemainingAmount);

                    if (allocationAmount2 > 0)
                    {
                        var paymentDebt = new PaymentDebt
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.Id,
                            DebtId = debt.Id,
                            PaidAmount = allocationAmount2,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PaymentDebts.Add(paymentDebt);
                        
                        // Borç kalan miktarını güncelle
                        debt.PaidAmount = (debt.PaidAmount ?? 0) + allocationAmount2;
                        debt.RemainingAmount = debt.Amount - debt.PaidAmount.Value;
                        debt.UpdatedAt = DateTime.UtcNow;

                        allocationResults.Add(new DebtAllocationResult
                        {
                            DebtId = debt.Id,
                            DebtDescription = debt.Description ?? $"{debt.Type} - {debt.PeriodYear}/{debt.PeriodMonth}",
                            AllocatedAmount = allocationAmount2,
                            RemainingDebtAmount = debt.RemainingAmount
                        });

                        remainingAmount -= allocationAmount2;
                    }
                }
            }

            // 3. Otomatik borç eşleştirme (kalan miktar varsa)
            if (request.AutoAllocate && remainingAmount > 0)
            {
                var tenantId = request.TenantId;
                if (tenantId.HasValue)
                {
                    var unpaidDebts = await _context.UtilityDebts
                        .Where(d => d.TenantId == tenantId && d.RemainingAmount > 0 && !d.IsDeleted)
                        .OrderBy(d => d.DueDate) // En eski borçtan başla
                        .ToListAsync(cancellationToken);

                    foreach (var debt in unpaidDebts)
                    {
                        if (remainingAmount <= 0) break;

                        var allocationAmount = Math.Min(remainingAmount, debt.RemainingAmount);

                        var paymentDebt = new PaymentDebt
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.Id,
                            DebtId = debt.Id,
                            PaidAmount = allocationAmount,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PaymentDebts.Add(paymentDebt);
                        
                        // Borç kalan miktarını güncelle
                        debt.PaidAmount = (debt.PaidAmount ?? 0) + allocationAmount;
                        debt.RemainingAmount = debt.Amount - debt.PaidAmount.Value;
                        debt.UpdatedAt = DateTime.UtcNow;

                        allocationResults.Add(new DebtAllocationResult
                        {
                            DebtId = debt.Id,
                            DebtDescription = debt.Description ?? $"{debt.Type} - {debt.PeriodYear}/{debt.PeriodMonth}",
                            AllocatedAmount = allocationAmount,
                            RemainingDebtAmount = debt.RemainingAmount
                        });

                        remainingAmount -= allocationAmount;
                    }
                }
            }

            // 4. Kalan miktar varsa avans hesabına ekle
            if (remainingAmount > 0 && effectiveTenantId.HasValue)
            {
                var advanceAccount = await _context.AdvanceAccounts
                    .FirstOrDefaultAsync(aa => aa.TenantId == effectiveTenantId && !aa.IsDeleted, cancellationToken);

                if (advanceAccount == null)
                {
                    // Yeni avans hesabı oluştur
                    advanceAccount = new AdvanceAccount
                    {
                        Id = Guid.NewGuid(),
                        TenantId = effectiveTenantId.Value,
                        Balance = remainingAmount,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AdvanceAccounts.Add(advanceAccount);
                }
                else
                {
                    // Mevcut avans hesabını güncelle
                    advanceAccount.Balance += remainingAmount;
                    advanceAccount.UpdatedAt = DateTime.UtcNow;
                }

                allocationResults.Add(new DebtAllocationResult
                {
                    DebtId = Guid.Empty,
                    DebtDescription = "Avans Hesabı",
                    AllocatedAmount = remainingAmount,
                    RemainingDebtAmount = 0
                });
            }

            // 5. Değişiklikleri kaydet
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Ödeme başarıyla oluşturuldu: {PaymentId}, Toplam: {Amount}, Eşleştirilen: {Allocated}", 
                payment.Id, request.Amount, request.Amount - remainingAmount);

            var result = new PaymentWithAllocationDto
            {
                Payment = new PaymentDto
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    Type = payment.Type,
                    Status = payment.Status,
                    PaymentDate = payment.PaymentDate,
                    Description = payment.Description,
                    ReceiptNumber = payment.ReceiptNumber,
                    OwnerId = payment.OwnerId,
                    TenantId = payment.TenantId,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt
                },
                Allocations = allocationResults,
                TotalAllocated = request.Amount - remainingAmount,
                RemainingAmount = remainingAmount
            };

            return Result<PaymentWithAllocationDto>.Success(result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Ödeme oluşturulurken hata oluştu: {Amount}", request.Amount);
            return Result<PaymentWithAllocationDto>.Failure($"Ödeme oluşturulamadı: {ex.Message}");
        }
    }
}

public class PaymentWithAllocationDto
{
    public PaymentDto Payment { get; set; } = null!;
    public List<DebtAllocationResult> Allocations { get; set; } = new();
    public decimal TotalAllocated { get; set; }
    public decimal RemainingAmount { get; set; }
}

public class DebtAllocationResult
{
    public Guid DebtId { get; set; }
    public string DebtDescription { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal RemainingDebtAmount { get; set; }
} 