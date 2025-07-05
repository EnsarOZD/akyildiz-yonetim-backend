using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendPaymentConfirmationAsync(
        Guid paymentId,
        decimal amount,
        PaymentType type,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
    
    Task SendDebtAllocationNotificationAsync(
        Guid tenantId,
        List<DebtAllocationInfo> allocations,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
    
    Task SendAdvanceAccountUsageNotificationAsync(
        Guid tenantId,
        decimal amount,
        decimal newBalance,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
    
    Task SendOverdueDebtReminderAsync(
        Guid tenantId,
        List<UtilityDebt> overdueDebts,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default);
}

public record DebtAllocationInfo
{
    public Guid DebtId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal AllocatedAmount { get; init; }
    public decimal RemainingAmount { get; init; }
} 