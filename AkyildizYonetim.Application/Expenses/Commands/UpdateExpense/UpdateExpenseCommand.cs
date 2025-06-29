using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Expenses.Commands.UpdateExpense;

public record UpdateExpenseCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public ExpenseType Type { get; init; }
    public DateTime ExpenseDate { get; init; }
    public string? Description { get; init; }
    public string? ReceiptNumber { get; init; }
    public Guid? OwnerId { get; init; }
}

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);

        if (expense == null)
            return Result.Failure("Gider bulunamadı.");

        expense.Title = request.Title;
        expense.Amount = request.Amount;
        expense.Type = request.Type;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Description = request.Description;
        expense.ReceiptNumber = request.ReceiptNumber;
        expense.OwnerId = request.OwnerId;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 