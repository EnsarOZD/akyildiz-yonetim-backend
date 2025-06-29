using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.Expenses.Commands.CreateExpense;

public record CreateExpenseCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public ExpenseType Type { get; init; }
    public DateTime ExpenseDate { get; init; }
    public string? Description { get; init; }
    public string? ReceiptNumber { get; init; }
    public Guid? OwnerId { get; init; }
}

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Amount = request.Amount,
            Type = request.Type,
            ExpenseDate = request.ExpenseDate,
            Description = request.Description,
            ReceiptNumber = request.ReceiptNumber,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(expense.Id);
    }
} 