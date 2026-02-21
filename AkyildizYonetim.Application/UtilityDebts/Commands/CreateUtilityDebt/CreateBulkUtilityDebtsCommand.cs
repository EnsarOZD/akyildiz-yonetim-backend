using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;

public record CreateBulkUtilityDebtsCommand : IRequest<Result<int>>
{
    public List<CreateUtilityDebtCommand> Debts { get; init; } = new();
}

public class CreateBulkUtilityDebtsCommandHandler : IRequestHandler<CreateBulkUtilityDebtsCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateBulkUtilityDebtsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateBulkUtilityDebtsCommand request, CancellationToken ct)
    {
        if (request.Debts == null || !request.Debts.Any())
            return Result<int>.Failure("Kaydedilecek borç bulunamadı.");

        var entities = new List<UtilityDebt>();

        foreach (var req in request.Debts)
        {
            var due = req.DueDate == default
                ? new DateTime(req.PeriodYear, req.PeriodMonth, 1).AddMonths(1).AddDays(9)
                : req.DueDate;

            var remaining = req.RemainingAmount == 0 ? req.Amount : req.RemainingAmount;

            entities.Add(new UtilityDebt
            {
                Id = Guid.NewGuid(),
                FlatId = req.FlatId,
                Type = req.Type,
                PeriodYear = req.PeriodYear,
                PeriodMonth = req.PeriodMonth,
                Amount = req.Amount,
                RemainingAmount = remaining,
                Status = req.Status,
                PaidAmount = req.PaidAmount,
                DueDate = due,
                PaidDate = req.PaidDate,
                Description = req.Description,
                TenantId = req.TenantId,
                OwnerId = req.OwnerId,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.UtilityDebts.AddRange(entities);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(entities.Count);
    }
}
