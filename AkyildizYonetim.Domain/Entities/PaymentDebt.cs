namespace AkyildizYonetim.Domain.Entities;

public class PaymentDebt : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid DebtId { get; set; }
    
    public decimal PaidAmount { get; set; }
    
    // Navigation properties
    public virtual Payment Payment { get; set; } = null!;
    public virtual UtilityDebt Debt { get; set; } = null!;
} 