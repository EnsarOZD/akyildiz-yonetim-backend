using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Entities.Enums;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IUtilityConfigurationService
{
    Task<UtilityPricing> GetPricingAsync(int year, int month, MeterType type);
    Task<decimal> GetVatRateAsync(int year, int month);
    Task<decimal> GetBtvRateAsync(int year, int month);
    Task<UtilityPricingConfiguration?> GetActivePricingConfigurationAsync(int year, int month, MeterType type);
}

public class UtilityPricing
{
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public decimal BtvRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public MeterType MeterType { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}
