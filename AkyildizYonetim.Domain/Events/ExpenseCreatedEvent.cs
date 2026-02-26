using MediatR;

namespace AkyildizYonetim.Domain.Events;

public class ExpenseCreatedEvent : INotification
{
    public Guid ExpenseId { get; }
    public string Title { get; }
    public decimal Amount { get; }
    public string? Description { get; }

    public ExpenseCreatedEvent(Guid expenseId, string title, decimal amount, string? description = null)
    {
        ExpenseId = expenseId;
        Title = title;
        Amount = amount;
        Description = description;
    }
}
