using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.PatchUtilityDebt;

public record PatchUtilityDebtCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public decimal? Amount { get; init; }
    public decimal? KdvHaric { get; init; }
    public decimal? KdvDahil { get; init; }
    public decimal? RemainingAmount { get; init; }
    public DebtStatus? Status { get; init; }
    public decimal? PaidAmount { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? Description { get; init; }
    public bool? IsPaid { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public int? PeriodYear { get; init; }
    public int? PeriodMonth { get; init; }
    public DateTime? DueDate { get; init; }
}

public class PatchUtilityDebtCommandHandler : IRequestHandler<PatchUtilityDebtCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public PatchUtilityDebtCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(PatchUtilityDebtCommand request, CancellationToken cancellationToken)
    {
        var debt = await _context.UtilityDebts
            .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);

        if (debt == null)
            return Result.Failure("Borç kaydı bulunamadı.");

        if (request.KdvDahil.HasValue) debt.Amount = request.KdvDahil.Value;
        else if (request.Amount.HasValue) debt.Amount = request.Amount.Value;

        if (request.RemainingAmount.HasValue) debt.RemainingAmount = request.RemainingAmount.Value;
        if (request.Status.HasValue) debt.Status = request.Status.Value;
        if (request.PaidAmount.HasValue) debt.PaidAmount = request.PaidAmount.Value;
        if (request.PaidDate.HasValue) debt.PaidDate = request.PaidDate.Value;
        if (request.Description != null) debt.Description = request.Description;
        if (request.TenantId.HasValue) debt.TenantId = request.TenantId;
        if (request.OwnerId.HasValue) debt.OwnerId = request.OwnerId;
        if (request.PeriodYear.HasValue) debt.PeriodYear = request.PeriodYear.Value;
        if (request.PeriodMonth.HasValue) debt.PeriodMonth = request.PeriodMonth.Value;
        if (request.DueDate.HasValue) debt.DueDate = request.DueDate.Value;

        if (request.IsPaid.HasValue)
        {
            debt.Status = request.IsPaid.Value ? DebtStatus.Paid : DebtStatus.Unpaid;
            if (request.IsPaid.Value && debt.RemainingAmount > 0)
            {
                debt.PaidAmount = (debt.PaidAmount ?? 0) + debt.RemainingAmount;
                debt.RemainingAmount = 0;
                debt.PaidDate ??= DateTime.UtcNow;
            }
        }

        if (!request.RemainingAmount.HasValue && (request.KdvDahil.HasValue || request.Amount.HasValue || request.PaidAmount.HasValue))
        {
            debt.RemainingAmount = debt.Amount - (debt.PaidAmount ?? 0);
        }

        debt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
