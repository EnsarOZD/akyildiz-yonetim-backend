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
    public DateTime? PaidDate { get; init; }
    public string? Description { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }
}

public class CreateUtilityDebtCommandHandler : IRequestHandler<CreateUtilityDebtCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateUtilityDebtCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<Guid>> Handle(CreateUtilityDebtCommand request, CancellationToken cancellationToken)
    {
        var debt = new UtilityDebt
        {
            Id = Guid.NewGuid(),
            FlatId = request.FlatId,
            Type = request.Type,
            PeriodYear = request.PeriodYear,
            PeriodMonth = request.PeriodMonth,
            Amount = request.Amount,
            Status = request.Status,
            PaidAmount = request.PaidAmount,
            PaidDate = request.PaidDate,
            Description = request.Description,
            TenantId = request.TenantId,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow
        };
        _context.UtilityDebts.Add(debt);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(debt.Id);
    }
} 