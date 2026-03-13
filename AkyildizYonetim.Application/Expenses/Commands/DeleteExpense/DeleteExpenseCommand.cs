using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Expenses.Commands.DeleteExpense;

public record DeleteExpenseCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (expense == null)
            return Result.Failure("Gider bulunamadı.");

        _context.Expenses.Remove(expense);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 