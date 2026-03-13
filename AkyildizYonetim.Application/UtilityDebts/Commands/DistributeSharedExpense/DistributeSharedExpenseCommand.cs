using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.DistributeSharedExpense;

public record DistributeSharedExpenseCommand : IRequest<Result<DistributeSharedExpenseResponse>>
{
    public string Period { get; init; } = string.Empty;
    public string UtilityType { get; init; } = string.Empty;
}

public class DistributeSharedExpenseResponse
{
    public string Message { get; set; } = string.Empty;
    public decimal SharedExpenseAmount { get; set; }
    public int UnitCount { get; set; }
    public decimal AmountPerFlat { get; set; }
    public int CreatedDebtsCount { get; set; }
}

public class DistributeSharedExpenseCommandHandler : IRequestHandler<DistributeSharedExpenseCommand, Result<DistributeSharedExpenseResponse>>
{
    private readonly IApplicationDbContext _context;

    public DistributeSharedExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DistributeSharedExpenseResponse>> Handle(DistributeSharedExpenseCommand request, CancellationToken cancellationToken)
    {
        // Dönem bilgisini parse et
        if (!DateTime.TryParseExact(request.Period, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime periodDate))
        {
            return Result<DistributeSharedExpenseResponse>.Failure("Geçersiz dönem formatı. YYYY-MM formatında olmalı.");
        }

        // Utility type'ı enum'a çevir
        if (!Enum.TryParse<DebtType>(request.UtilityType, out DebtType utilityType))
        {
            return Result<DistributeSharedExpenseResponse>.Failure("Geçersiz gider tipi.");
        }

        // Ortak gider kaydını bul
        var sharedExpense = await _context.UtilityDebts
            .AsNoTracking() // Optimization
            .Where(d => d.PeriodYear == periodDate.Year && 
                       d.PeriodMonth == periodDate.Month && 
                       d.Type == utilityType &&
                       (d.Description == "Ortak Alan" || d.Description == "Mescit"))
            .FirstOrDefaultAsync(cancellationToken);

        if (sharedExpense == null)
        {
            return Result<DistributeSharedExpenseResponse>.Failure("Bu dönem için ortak gider kaydı bulunamadı.");
        }

        // Aktif (dolu) üniteleri getir
        var activeFlats = await _context.Flats
            .AsNoTracking() // Optimization
            .Where(f => !f.IsDeleted && f.TenantId != null && f.IsOccupied)
            .Include(f => f.Tenant)
            .ToListAsync(cancellationToken);

        if (!activeFlats.Any())
        {
            return Result<DistributeSharedExpenseResponse>.Failure("Dolu ünite (aktif kiracı) bulunamadı.");
        }

        // Ünite başına düşen tutarı hesapla
        decimal amountPerFlat = sharedExpense.Amount / activeFlats.Count;

        // Her ünite için borç kaydı oluştur
        var createdDebtsCount = 0;
        foreach (var flat in activeFlats)
        {
            var defaultDueDate = new DateTime(periodDate.Year, periodDate.Month, 1).AddDays(9);

            var debt = new UtilityDebt
            {
                Id = Guid.NewGuid(),
                FlatId = flat.Id,
                Type = utilityType,
                PeriodYear = periodDate.Year,
                PeriodMonth = periodDate.Month,
                Amount = amountPerFlat,
                RemainingAmount = amountPerFlat,
                Status = DebtStatus.Unpaid,
                DueDate = defaultDueDate,
                Description = $"Ortak {request.UtilityType} Payı - {flat.Code} ({flat.Tenant?.CompanyName})",
                TenantId = flat.TenantId,
                OwnerId = flat.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UtilityDebts.Add(debt);
            createdDebtsCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<DistributeSharedExpenseResponse>.Success(new DistributeSharedExpenseResponse
        {
            Message = "Ortak gider başarıyla paylaştırıldı",
            SharedExpenseAmount = sharedExpense.Amount,
            UnitCount = activeFlats.Count,
            AmountPerFlat = amountPerFlat,
            CreatedDebtsCount = createdDebtsCount
        });
    }
}
