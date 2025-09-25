using AkyildizYonetim.Domain.Entities.Enums;

namespace AkyildizYonetim.Domain.Entities;

public class UtilityPricingConfiguration : BaseEntity
{
    public MeterType MeterType { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public decimal BtvRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
