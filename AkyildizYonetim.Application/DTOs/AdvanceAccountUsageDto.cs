using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.DTOs;

public class AdvanceAccountUsageDto
{
    public Guid PaymentId { get; set; }
    public Guid TenantId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public List<DebtPaymentResult> DebtPayments { get; set; } = new();
    public string? Description { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class DebtPaymentResult
{
    public Guid DebtId { get; set; }
    public string DebtDescription { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal RemainingDebtAmount { get; set; }
    public DebtStatus DebtStatus { get; set; }
} 