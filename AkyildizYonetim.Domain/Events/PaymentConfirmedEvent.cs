using MediatR;

namespace AkyildizYonetim.Domain.Events;

public class PaymentConfirmedEvent : INotification
{
    public Guid PaymentId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public DateTime OccurredAt { get; }

    public PaymentConfirmedEvent(Guid paymentId, Guid userId, decimal amount)
    {
        PaymentId = paymentId;
        UserId = userId;
        Amount = amount;
        OccurredAt = DateTime.UtcNow;
    }
}
