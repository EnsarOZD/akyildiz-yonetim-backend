using MediatR;

namespace AkyildizYonetim.Domain.Events;

public class PaymentCreatedEvent : INotification
{
    public Guid PaymentId { get; }
    public Guid? TenantId { get; }
    public decimal Amount { get; }
    public string? CreatedByName { get; }

    public PaymentCreatedEvent(Guid paymentId, Guid? tenantId, decimal amount, string? createdByName = null)
    {
        PaymentId = paymentId;
        TenantId = tenantId;
        Amount = amount;
        CreatedByName = createdByName;
    }
}
