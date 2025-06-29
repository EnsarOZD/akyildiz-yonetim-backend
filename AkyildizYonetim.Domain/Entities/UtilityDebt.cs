namespace AkyildizYonetim.Domain.Entities;

public enum DebtType
{
    Aidat,
    Electricity,
    Water
}

public enum DebtStatus
{
    Unpaid,
    Partial,
    Paid
}

public class UtilityDebt : BaseEntity
{
    public Guid FlatId { get; set; }
    public DebtType Type { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal Amount { get; set; }
    public DebtStatus Status { get; set; } = DebtStatus.Unpaid;
    public decimal? PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    // Navigation
    public virtual Flat Flat { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
    public virtual Owner? Owner { get; set; }
} 