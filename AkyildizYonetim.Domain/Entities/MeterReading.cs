namespace AkyildizYonetim.Domain.Entities;

public enum MeterType
{
    Electricity,
    Water
}

public class MeterReading : BaseEntity
{
    public Guid FlatId { get; set; }
    public MeterType Type { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal ReadingValue { get; set; } // Sayaç değeri
    public decimal Consumption { get; set; } // Tüketim (opsiyonel)
    public DateTime ReadingDate { get; set; }
    public string? Note { get; set; }
    // Navigation
    public virtual Flat Flat { get; set; } = null!;
} 