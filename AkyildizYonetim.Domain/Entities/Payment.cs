namespace AkyildizYonetim.Domain.Entities;

public enum PaymentType
{
    Rent,
    Dues,
    Utility,
    Other
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Cancelled
}

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    
    // Foreign keys
    public Guid? OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    
    // Navigation properties
    public virtual Owner? Owner { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<PaymentDebt> PaymentDebts { get; set; } = new List<PaymentDebt>();
} 