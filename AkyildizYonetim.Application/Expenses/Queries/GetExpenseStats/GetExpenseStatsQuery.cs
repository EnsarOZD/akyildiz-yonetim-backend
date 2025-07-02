using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Expenses.Queries.GetExpenseStats;

public record GetExpenseStatsQuery : IRequest<Result<ExpenseStatsDto>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetExpenseStatsQueryHandler : IRequestHandler<GetExpenseStatsQuery, Result<ExpenseStatsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExpenseStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExpenseStatsDto>> Handle(GetExpenseStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Expenses.Where(e => !e.IsDeleted);

            if (request.StartDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= request.EndDate.Value);

            var totalAmount = await query.SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0;
            var totalCount = await query.CountAsync(cancellationToken);

            // Bu ayki istatistikler
            var currentMonth = DateTime.Now;
            var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var thisMonthQuery = _context.Expenses
                .Where(e => !e.IsDeleted && e.ExpenseDate >= startOfMonth && e.ExpenseDate <= endOfMonth);

            var thisMonthAmount = await thisMonthQuery.SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0;
            var thisMonthCount = await thisMonthQuery.CountAsync(cancellationToken);

            // Tip bazında istatistikler
            var typeStats = await query
                .GroupBy(e => e.Type)
                .Select(g => new ExpenseTypeStatsDto
                {
                    Type = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(e => (decimal?)e.Amount) ?? 0
                })
                .ToListAsync(cancellationToken);

            var stats = new ExpenseStatsDto
            {
                TotalAmount = totalAmount,
                TotalCount = totalCount,
                ThisMonthAmount = thisMonthAmount,
                ThisMonthCount = thisMonthCount,
                TypeStats = typeStats
            };

            return Result<ExpenseStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetExpenseStatsQueryHandler] Hata: {ex.Message} - {ex.StackTrace}");
            return Result<ExpenseStatsDto>.Failure("Gider istatistikleri alınırken hata oluştu: " + ex.Message);
        }
    }
}

public class ExpenseStatsDto
{
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public decimal ThisMonthAmount { get; set; }
    public int ThisMonthCount { get; set; }
    public List<ExpenseTypeStatsDto> TypeStats { get; set; } = new();
}

public class ExpenseTypeStatsDto
{
    public Domain.Entities.ExpenseType Type { get; set; }
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
} 