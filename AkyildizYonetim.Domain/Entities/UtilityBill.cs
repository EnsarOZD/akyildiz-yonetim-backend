namespace AkyildizYonetim.Domain.Entities;

public enum UtilityType
{
    Electricity,
    Water
}

public class UtilityBill : BaseEntity
{
    public UtilityType Type { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BillDate { get; set; }
    public string? Description { get; set; }
} 