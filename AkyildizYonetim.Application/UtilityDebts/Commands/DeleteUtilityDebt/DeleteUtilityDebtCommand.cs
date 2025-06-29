using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.DeleteUtilityDebt;

public record DeleteUtilityDebtCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteUtilityDebtCommandHandler : IRequestHandler<DeleteUtilityDebtCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteUtilityDebtCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result> Handle(DeleteUtilityDebtCommand request, CancellationToken cancellationToken)
    {
        var debt = await _context.UtilityDebts.FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);
        if (debt == null)
            return Result.Failure("Borç kaydı bulunamadı.");
        debt.IsDeleted = true;
        debt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 