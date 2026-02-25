using MediatR;

namespace AkyildizYonetim.Domain.Events;

public class DebtCreatedEvent : INotification
{
    public Guid DebtId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public string Type { get; }
    public DateTime OccurredAt { get; }

    public DebtCreatedEvent(Guid debtId, Guid userId, decimal amount, string type)
    {
        DebtId = debtId;
        UserId = userId;
        Amount = amount;
        Type = type;
        OccurredAt = DateTime.UtcNow;
    }
}
