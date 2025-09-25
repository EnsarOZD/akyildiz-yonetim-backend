using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Entities.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands.ApplySharedConsumption;

public record ApplySharedConsumptionCommand : IRequest<Result<ApplySharedConsumptionResult>>
{
    public string OperationId { get; init; } = string.Empty;
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public DateTime DueDate { get; init; }
    public MeterType MeterType { get; init; } = MeterType.Electricity; // Varsayılan elektrik
    public decimal VatRate { get; init; } // KDV oranı (%)
    public decimal BtvRate { get; init; } // BTV oranı (%)
    public decimal DefaultUnitPrice { get; init; } // Birim fiyat (TL/kWh veya TL/m³)
    public List<SharedConsumptionItem> Items { get; init; } = new();
}

public record SharedConsumptionItem
{
	public Guid FlatId { get; init; }
    public int ShareCount { get; init; }
    public decimal DistributedConsumption { get; init; }
    public decimal? UnitPrice { get; init; } // Opsiyonel: Özel birim fiyat
}

public class ApplySharedConsumptionResult
{
    public string OperationId { get; set; } = string.Empty;
    public int CreatedMeterReadings { get; set; }
    public int CreatedUtilityDebts { get; set; }
    public decimal TotalAmount { get; set; }
    public PricingSummary PricingUsed { get; set; } = new();
    public List<CreatedItem> CreatedItems { get; set; } = new();
}

public class PricingSummary
{
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public decimal BtvRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreatedItem
{
    public Guid FlatId { get; set; }
    public string FlatNumber { get; set; } = string.Empty;
    public Guid MeterReadingId { get; set; }
    public Guid UtilityDebtId { get; set; }
    public decimal Consumption { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public class ApplySharedConsumptionCommandValidator : AbstractValidator<ApplySharedConsumptionCommand>
{
    public ApplySharedConsumptionCommandValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty().WithMessage("İşlem ID'si zorunludur");

        RuleFor(x => x.PeriodYear)
            .GreaterThan(2000).WithMessage("Geçerli bir yıl giriniz")
            .LessThan(2100).WithMessage("Geçerli bir yıl giriniz");

        RuleFor(x => x.PeriodMonth)
            .GreaterThan(0).WithMessage("Geçerli bir ay giriniz")
            .LessThan(13).WithMessage("Geçerli bir ay giriniz");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Vade tarihi zorunludur");

        RuleFor(x => x.VatRate)
            .GreaterThanOrEqualTo(0).WithMessage("KDV oranı 0'dan küçük olamaz")
            .LessThanOrEqualTo(100).WithMessage("KDV oranı 100'den büyük olamaz");

        RuleFor(x => x.BtvRate)
            .GreaterThanOrEqualTo(0).WithMessage("BTV oranı 0'dan küçük olamaz")
            .LessThanOrEqualTo(100).WithMessage("BTV oranı 100'den büyük olamaz");

        RuleFor(x => x.DefaultUnitPrice)
            .GreaterThan(0).WithMessage("Birim fiyat 0'dan büyük olmalıdır");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("En az bir daire seçilmelidir");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.FlatId)
                .NotEmpty().WithMessage("Daire seçimi zorunludur");

            item.RuleFor(x => x.ShareCount)
                .GreaterThan(0).WithMessage("Hisse sayısı 0'dan büyük olmalıdır");

            item.RuleFor(x => x.DistributedConsumption)
                .GreaterThanOrEqualTo(0).WithMessage("Dağıtılan tüketim 0'dan küçük olamaz");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Birim fiyat 0'dan büyük olmalıdır")
                .When(x => x.UnitPrice.HasValue);
        });
    }
}

public class ApplySharedConsumptionCommandHandler 
    : IRequestHandler<ApplySharedConsumptionCommand, Result<ApplySharedConsumptionResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUtilityConfigurationService _utilityConfigService;

    public ApplySharedConsumptionCommandHandler(
        IApplicationDbContext context,
        IUtilityConfigurationService utilityConfigService)
    {
        _context = context;
        _utilityConfigService = utilityConfigService;
    }

    public async Task<Result<ApplySharedConsumptionResult>> Handle(
        ApplySharedConsumptionCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Aynı işlem ID'si ile daha önce işlem yapılmış mı kontrol et
            var existingOperation = await _context.MeterReadings
                .Where(mr => mr.Note != null && mr.Note.Contains($"OperationId:{request.OperationId}"))
                .AnyAsync(cancellationToken);

            if (existingOperation)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ApplySharedConsumptionResult>.Failure("Bu işlem daha önce uygulanmış");
            }

            // 2. Dönem için fiyatlandırma bilgilerini al
            var pricing = await _utilityConfigService.GetPricingAsync(
                request.PeriodYear, 
                request.PeriodMonth, 
                request.MeterType);

            // 3. Request'ten gelen değerler varsa onları kullan, yoksa config'den al
            var vatRate = request.VatRate > 0 ? request.VatRate : pricing.VatRate;
            var btvRate = request.BtvRate > 0 ? request.BtvRate : pricing.BtvRate;
            var unitPrice = request.DefaultUnitPrice > 0 ? request.DefaultUnitPrice : pricing.UnitPrice;

            var result = new ApplySharedConsumptionResult
            {
                OperationId = request.OperationId,
                CreatedItems = new List<CreatedItem>(),
                PricingUsed = new PricingSummary
                {
                    UnitPrice = unitPrice,
                    VatRate = vatRate,
                    BtvRate = btvRate,
                    EffectiveDate = pricing.EffectiveDate,
                    Description = pricing.Description
                }
            };

            decimal totalAmount = 0;

            // 4. Her daire için sayaç okuması ve borç kaydı oluştur
            foreach (var item in request.Items)
            {
                // Daire bilgisini al
                var flat = await _context.Flats
                    .FirstOrDefaultAsync(f => f.Id == item.FlatId && !f.IsDeleted, cancellationToken);

                if (flat == null)
                    continue;

                // Kiracı bilgisini al
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Id == flat.TenantId && !t.IsDeleted, cancellationToken);

                // Birim fiyatı belirle (item'dan gelen özel fiyat varsa onu kullan)
                var itemUnitPrice = item.UnitPrice ?? unitPrice;

                // Tutarı hesapla (KDV ve BTV dahil)
                var baseAmount = item.DistributedConsumption * itemUnitPrice;
                var vatAmount = baseAmount * (vatRate / 100);
                var btvAmount = baseAmount * (btvRate / 100);
                var totalItemAmount = baseAmount + vatAmount + btvAmount;

                totalAmount += totalItemAmount;

                // 3. Sayaç okuması oluştur
                var meterReadingId = Guid.NewGuid();
                var meterReading = new MeterReading
                {
                    Id = meterReadingId,
                    FlatId = item.FlatId,
                    Type = request.MeterType, // Ortak tüketim tipi
                    PeriodYear = request.PeriodYear,
                    PeriodMonth = request.PeriodMonth,
                    ReadingValue = 0, // Ortak tüketim için okuma değeri 0
                    Consumption = item.DistributedConsumption,
                    ReadingDate = DateTime.UtcNow,
                    Note = $"Ortak Alan Tüketimi - OperationId:{request.OperationId}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.MeterReadings.Add(meterReading);

                // 4. Borç kaydı oluştur
                var utilityDebtId = Guid.NewGuid();
                var debtType = request.MeterType == MeterType.Electricity ? DebtType.Electricity : DebtType.Water;
                var utilityDebt = new UtilityDebt
                {
                    Id = utilityDebtId,
                    FlatId = item.FlatId,
                    Type = debtType,
                    PeriodYear = request.PeriodYear,
                    PeriodMonth = request.PeriodMonth,
                    Amount = totalItemAmount,
                    Status = DebtStatus.Unpaid,
                    PaidAmount = null,
                    PaidDate = null,
                    DueDate = request.DueDate,
                    Description = $"Ortak Alan {(request.MeterType == MeterType.Electricity ? "Elektrik" : "Su")} Payı - {flat.Code}",
                    TenantId = tenant?.Id,
                    OwnerId = flat.OwnerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    IsDeleted = false
                };

                _context.UtilityDebts.Add(utilityDebt);

                    // 5. Sonuç listesine ekle
                    result.CreatedItems.Add(new CreatedItem
                    {
                        FlatId = item.FlatId,
                        FlatNumber = flat.Code, // ApartmentNumber yerine Code kullan
                        MeterReadingId = meterReadingId,
                        UtilityDebtId = utilityDebtId,
                        Consumption = item.DistributedConsumption,
                        UnitPrice = itemUnitPrice,
                        Amount = totalItemAmount
                    });

                result.CreatedMeterReadings++;
                result.CreatedUtilityDebts++;
            }

            result.TotalAmount = totalAmount;

            // 6. Değişiklikleri kaydet
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<ApplySharedConsumptionResult>.Success(result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<ApplySharedConsumptionResult>.Failure($"Ortak tüketim uygulanırken hata oluştu: {ex.Message}");
        }
    }
}