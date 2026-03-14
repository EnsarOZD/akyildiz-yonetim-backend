using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.DeleteBulkUtilityDebts;

public record DeleteBulkUtilityDebtsCommand : IRequest<Result>
{
    public List<Guid> Ids { get; init; } = new();
}

public class DeleteBulkUtilityDebtsCommandHandler : IRequestHandler<DeleteBulkUtilityDebtsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteBulkUtilityDebtsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBulkUtilityDebtsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || !request.Ids.Any())
            return Result.Failure("Silinecek aidat/borç bulunamadı.");

        var debts = await _context.UtilityDebts
            .Where(d => request.Ids.Contains(d.Id))
            .ToListAsync(cancellationToken);

        if (!debts.Any())
            return Result.Failure("Belirtilen kayıtlar bulunamadı.");

        _context.UtilityDebts.RemoveRange(debts);

        // Also remove related expenses if applicable
        var expenseIds = debts
            .Where(x => x.ExpenseId.HasValue)
            .Select(x => x.ExpenseId!.Value)
            .Distinct()
            .ToList();

        if (expenseIds.Any())
        {
            var relatedExpenses = await _context.Expenses
                .Where(e => expenseIds.Contains(e.Id))
                .ToListAsync(cancellationToken);
            _context.Expenses.RemoveRange(relatedExpenses);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
