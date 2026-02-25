using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;

public record CreateUtilityDebtCommand : IRequest<Result<Guid>>
{
    public Guid FlatId { get; init; }
    public DebtType Type { get; init; }
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public decimal Amount { get; init; }
    public DebtStatus Status { get; init; } = DebtStatus.Unpaid;
    public decimal? PaidAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? Description { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }
}

public class CreateUtilityDebtCommandHandler : IRequestHandler<CreateUtilityDebtCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    public CreateUtilityDebtCommandHandler(IApplicationDbContext context, IMediator mediator) 
    { 
        _context = context; 
        _mediator = mediator;
    }
    public async Task<Result<Guid>> Handle(CreateUtilityDebtCommand request, CancellationToken ct)
{
    var due = request.DueDate == default
        ? new DateTime(request.PeriodYear, request.PeriodMonth, 1).AddMonths(1).AddDays(9)
        : request.DueDate;

    var remaining = request.RemainingAmount == 0 ? request.Amount : request.RemainingAmount;

    var debt = new UtilityDebt
    {
        Id = Guid.NewGuid(),
        FlatId = request.FlatId,
        Type = request.Type,
        PeriodYear = request.PeriodYear,
        PeriodMonth = request.PeriodMonth,
        Amount = request.Amount,
        RemainingAmount = remaining,
        Status = request.Status,
        PaidAmount = request.PaidAmount,
        DueDate = due,
        PaidDate = request.PaidDate,
        Description = request.Description,
        TenantId = request.TenantId,
        OwnerId = request.OwnerId,
        CreatedAt = DateTime.UtcNow
    };

    _context.UtilityDebts.Add(debt);
    await _context.SaveChangesAsync(ct);

    // Trigger Notification if target is a Tenant
    if (debt.TenantId.HasValue)
    {
        await _mediator.Publish(new AkyildizYonetim.Domain.Events.DebtCreatedEvent(
            debt.Id, 
            debt.TenantId.Value, 
            debt.Amount, 
            debt.Type.ToString()), ct);
    }

    return Result<Guid>.Success(debt.Id);
}
} 