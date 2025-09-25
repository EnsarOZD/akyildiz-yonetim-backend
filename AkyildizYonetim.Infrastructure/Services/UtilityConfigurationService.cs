using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Infrastructure.Services;

public class UtilityConfigurationService : IUtilityConfigurationService
{
    private readonly IApplicationDbContext _context;

    public UtilityConfigurationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UtilityPricing> GetPricingAsync(int year, int month, MeterType type)
    {
        // 1. Önce spesifik dönem için arama yap
        var specificConfig = await _context.UtilityPricingConfigurations
            .Where(upc => !upc.IsDeleted 
                          && upc.IsActive 
                          && upc.MeterType == type 
                          && upc.Year == year 
                          && upc.Month == month
                          && upc.EffectiveDate <= DateTime.UtcNow
                          && (upc.ExpiryDate == null || upc.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(upc => upc.EffectiveDate)
            .FirstOrDefaultAsync();

        if (specificConfig != null)
        {
            return new UtilityPricing
            {
                UnitPrice = specificConfig.UnitPrice,
                VatRate = specificConfig.VatRate,
                BtvRate = specificConfig.BtvRate,
                EffectiveDate = specificConfig.EffectiveDate,
                ExpiryDate = specificConfig.ExpiryDate,
                Description = specificConfig.Description,
                MeterType = specificConfig.MeterType,
                Year = specificConfig.Year,
                Month = specificConfig.Month
            };
        }

        // 2. Spesifik dönem bulunamazsa, genel aktif konfigürasyon ara
        var generalConfig = await _context.UtilityPricingConfigurations
            .Where(upc => !upc.IsDeleted 
                          && upc.IsActive 
                          && upc.MeterType == type 
                          && upc.EffectiveDate <= DateTime.UtcNow
                          && (upc.ExpiryDate == null || upc.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(upc => upc.EffectiveDate)
            .FirstOrDefaultAsync();

        if (generalConfig != null)
        {
            return new UtilityPricing
            {
                UnitPrice = generalConfig.UnitPrice,
                VatRate = generalConfig.VatRate,
                BtvRate = generalConfig.BtvRate,
                EffectiveDate = generalConfig.EffectiveDate,
                ExpiryDate = generalConfig.ExpiryDate,
                Description = generalConfig.Description,
                MeterType = generalConfig.MeterType,
                Year = year,
                Month = month
            };
        }

        // 3. Hiçbir konfigürasyon bulunamazsa varsayılan değerler döndür
        return GetDefaultPricing(year, month, type);
    }

    public async Task<decimal> GetVatRateAsync(int year, int month)
    {
        var pricing = await GetPricingAsync(year, month, MeterType.Electricity);
        return pricing.VatRate;
    }

    public async Task<decimal> GetBtvRateAsync(int year, int month)
    {
        var pricing = await GetPricingAsync(year, month, MeterType.Electricity);
        return pricing.BtvRate;
    }

    public async Task<UtilityPricingConfiguration?> GetActivePricingConfigurationAsync(int year, int month, MeterType type)
    {
        return await _context.UtilityPricingConfigurations
            .Where(upc => !upc.IsDeleted 
                          && upc.IsActive 
                          && upc.MeterType == type 
                          && upc.Year == year 
                          && upc.Month == month
                          && upc.EffectiveDate <= DateTime.UtcNow
                          && (upc.ExpiryDate == null || upc.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(upc => upc.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    private UtilityPricing GetDefaultPricing(int year, int month, MeterType type)
    {
        // Varsayılan değerler - appsettings.json'dan da alınabilir
        var defaultUnitPrice = type == MeterType.Electricity ? 2.50m : 1.20m;
        
        return new UtilityPricing
        {
            UnitPrice = defaultUnitPrice,
            VatRate = 20.00m,
            BtvRate = 5.00m,
            EffectiveDate = DateTime.UtcNow,
            ExpiryDate = null,
            Description = "Varsayılan fiyatlandırma",
            MeterType = type,
            Year = year,
            Month = month
        };
    }
}
