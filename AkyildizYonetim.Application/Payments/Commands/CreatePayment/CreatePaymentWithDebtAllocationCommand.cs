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
    public DebtType? TargetDebtType { get; init; } // Filtrelenecek borç kategorisi
    public string? BankName { get; init; } // Banka adı
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
                Status = PaymentStatus.Completed,
                Method = !string.IsNullOrEmpty(request.BankName) ? PaymentMethod.BankTransfer : PaymentMethod.Cash,
                BankName = request.BankName,
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
                var debtIds = request.DebtAllocations.Select(a => a.DebtId).Distinct().ToList();
                
                // N+1 Fix & IDOR Protection: Fetch all related debts at once
                var debts = await _context.UtilityDebts
                    .Where(d => debtIds.Contains(d.Id) && !d.IsDeleted)
                    .Where(d => (effectiveTenantId.HasValue && d.TenantId == effectiveTenantId.Value) || 
                                (effectiveOwnerId.HasValue && d.OwnerId == effectiveOwnerId.Value))
                    .ToDictionaryAsync(d => d.Id, cancellationToken);
                
                if (debts.Count != debtIds.Count)
                {
                    return Result<PaymentWithAllocationDto>.Failure("Bazı borçlar bulunamadı veya yetkisiz bir borç ödenmeye çalışılıyor.");
                }

                foreach (var allocation in request.DebtAllocations)
                {
                    if (remainingAmount <= 0) break;

                    if (!debts.TryGetValue(allocation.DebtId, out var debt))
                        continue;

                    var requestedAllocationAmount = Math.Min(allocation.Amount, remainingAmount);
                    var finalAllocationAmount = Math.Min(requestedAllocationAmount, debt.RemainingAmount);

                    if (finalAllocationAmount > 0)
                    {
                        var paymentDebt = new PaymentDebt
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.Id,
                            DebtId = debt.Id,
                            PaidAmount = finalAllocationAmount,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PaymentDebts.Add(paymentDebt);
                        
                        // Borç kalan miktarını güncelle
                        debt.PaidAmount = (debt.PaidAmount ?? 0) + finalAllocationAmount;
                        debt.RemainingAmount = debt.Amount - debt.PaidAmount.Value;
                        debt.UpdatedAt = DateTime.UtcNow;

                        // Durum güncellemesi
                        if (debt.RemainingAmount <= 0) debt.Status = DebtStatus.Paid;
                        else if (debt.PaidAmount > 0) debt.Status = DebtStatus.Partial;

                        allocationResults.Add(new DebtAllocationResult
                        {
                            DebtId = debt.Id,
                            DebtDescription = debt.Description ?? $"{debt.Type} - {debt.PeriodYear}/{debt.PeriodMonth}",
                            AllocatedAmount = finalAllocationAmount,
                            RemainingDebtAmount = debt.RemainingAmount
                        });

                        remainingAmount -= finalAllocationAmount;
                    }
                }
            }

            // 3. Otomatik borç eşleştirme (kalan miktar varsa)
            if (request.AutoAllocate && remainingAmount > 0)
            {
                var tenantId = effectiveTenantId;
                var ownerId = effectiveOwnerId;

                if (tenantId.HasValue)
                {
                    _logger.LogInformation("Creating Automatic Allocation for Tenant: {TenantId}, TargetDebtType: {TargetType}", 
                        tenantId, request.TargetDebtType?.ToString() ?? "NULL");

                    var query = _context.UtilityDebts
                        .Where(d => d.TenantId == tenantId && d.RemainingAmount > 0 && !d.IsDeleted);

                    if (request.TargetDebtType.HasValue)
                    {
                        _logger.LogInformation("Filtering by DebtType: {TargetType}", request.TargetDebtType.Value);
                        query = query.Where(d => d.Type == request.TargetDebtType.Value);
                    }

                    var unpaidDebts = await query
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

                        // Durum güncellemesi
                        if (debt.RemainingAmount <= 0) debt.Status = DebtStatus.Paid;
                        else if (debt.PaidAmount > 0) debt.Status = DebtStatus.Partial;

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
                else if (ownerId.HasValue)
                {
                    _logger.LogInformation("Creating Automatic Allocation for Owner: {OwnerId}, TargetDebtType: {TargetType}", 
                        ownerId, request.TargetDebtType?.ToString() ?? "NULL");

                    var query = _context.UtilityDebts
                        .Where(d => d.OwnerId == ownerId && d.RemainingAmount > 0 && !d.IsDeleted);

                    if (request.TargetDebtType.HasValue)
                    {
                        _logger.LogInformation("Filtering by DebtType: {TargetType} for Owner", request.TargetDebtType.Value);
                        query = query.Where(d => d.Type == request.TargetDebtType.Value);
                    }

                    var unpaidDebts = await query
                        .OrderBy(d => d.DueDate)
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
                        
                        debt.PaidAmount = (debt.PaidAmount ?? 0) + allocationAmount;
                        debt.RemainingAmount = debt.Amount - debt.PaidAmount.Value;
                        debt.UpdatedAt = DateTime.UtcNow;

                        if (debt.RemainingAmount <= 0) debt.Status = DebtStatus.Paid;
                        else if (debt.PaidAmount > 0) debt.Status = DebtStatus.Partial;

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