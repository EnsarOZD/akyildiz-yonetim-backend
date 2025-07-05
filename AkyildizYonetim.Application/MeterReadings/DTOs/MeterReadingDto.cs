using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.MeterReadings.DTOs;

public class MeterReadingDto
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public string FlatNumber { get; set; } = string.Empty;
    public MeterType Type { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal ReadingValue { get; set; }
    public decimal Consumption { get; set; }
    public DateTime ReadingDate { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 