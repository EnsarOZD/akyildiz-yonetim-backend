using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.UpdateUtilityDebt;

public record UpdateUtilityDebtCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public DebtStatus Status { get; init; }
    public decimal? PaidAmount { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? Description { get; init; }
}

public class UpdateUtilityDebtCommandHandler : IRequestHandler<UpdateUtilityDebtCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateUtilityDebtCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result> Handle(UpdateUtilityDebtCommand request, CancellationToken cancellationToken)
    {
        var debt = await _context.UtilityDebts.FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);
        if (debt == null)
            return Result.Failure("Borç kaydı bulunamadı.");
        debt.Amount = request.Amount;
        debt.Status = request.Status;
        debt.PaidAmount = request.PaidAmount;
        debt.PaidDate = request.PaidDate;
        debt.Description = request.Description;
        debt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 