using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Dashboard.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery(string? DateFilter = null) : IRequest<Result<DashboardSummaryDto>>;

public class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(request.DateFilter);

            var paymentsQuery = _context.Payments.AsNoTracking().AsQueryable();
            var expensesQuery = _context.Expenses.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= startDate.Value);
            if (endDate.HasValue)
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= endDate.Value);
            if (startDate.HasValue)
                expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= startDate.Value);
            if (endDate.HasValue)
                expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= endDate.Value);

            var paymentsRaw = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new { p.Id, p.Amount, p.PaymentDate, p.Description, p.TenantId, p.OwnerId })
                .ToListAsync(ct);

            var expenses = await expensesQuery
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new DashboardExpenseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    Description = e.Description ?? e.Title
                })
                .ToListAsync(ct);

            var debtsRaw = await _context.UtilityDebts
                .AsNoTracking()
                .Where(d => d.Status != DebtStatus.Paid && d.RemainingAmount > 0)
                .Select(d => new
                {
                    d.Id, d.Amount, d.RemainingAmount, d.Type, d.Status, d.DueDate,
                    d.TenantId, d.OwnerId, d.PeriodMonth, d.PeriodYear
                })
                .ToListAsync(ct);

            // Enrich entity names in memory to avoid complex EF joins
            var tenantIds = debtsRaw.Where(d => d.TenantId.HasValue).Select(d => d.TenantId!.Value).Distinct().ToList();
            var ownerIds = debtsRaw.Where(d => d.OwnerId.HasValue).Select(d => d.OwnerId!.Value).Distinct().ToList();

            var tenantNames = tenantIds.Count > 0
                ? await _context.Tenants.AsNoTracking()
                    .Where(t => tenantIds.Contains(t.Id))
                    .Select(t => new { t.Id, Name = !string.IsNullOrEmpty(t.CompanyName) ? t.CompanyName : t.ContactPersonName })
                    .ToDictionaryAsync(t => t.Id, t => t.Name, ct)
                : new Dictionary<Guid, string>();

            var ownerNames = ownerIds.Count > 0
                ? await _context.Owners.AsNoTracking()
                    .Where(o => ownerIds.Contains(o.Id))
                    .Select(o => new { o.Id, Name = o.FirstName + " " + o.LastName })
                    .ToDictionaryAsync(o => o.Id, o => o.Name, ct)
                : new Dictionary<Guid, string>();

            // Enrich payment names
            var payTenantIds = paymentsRaw.Where(p => p.TenantId.HasValue).Select(p => p.TenantId!.Value).Distinct().Except(tenantIds).ToList();
            var payOwnerIds = paymentsRaw.Where(p => p.OwnerId.HasValue).Select(p => p.OwnerId!.Value).Distinct().Except(ownerIds).ToList();

            if (payTenantIds.Count > 0)
            {
                var extra = await _context.Tenants.AsNoTracking()
                    .Where(t => payTenantIds.Contains(t.Id))
                    .Select(t => new { t.Id, Name = !string.IsNullOrEmpty(t.CompanyName) ? t.CompanyName : t.ContactPersonName })
                    .ToDictionaryAsync(t => t.Id, t => t.Name, ct);
                foreach (var kv in extra) tenantNames[kv.Key] = kv.Value;
            }
            if (payOwnerIds.Count > 0)
            {
                var extra = await _context.Owners.AsNoTracking()
                    .Where(o => payOwnerIds.Contains(o.Id))
                    .Select(o => new { o.Id, Name = o.FirstName + " " + o.LastName })
                    .ToDictionaryAsync(o => o.Id, o => o.Name, ct);
                foreach (var kv in extra) ownerNames[kv.Key] = kv.Value;
            }

            var payments = paymentsRaw.Select(p => new DashboardPaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Description = p.Description,
                TenantId = p.TenantId,
                OwnerId = p.OwnerId,
                TenantName = p.TenantId.HasValue && tenantNames.TryGetValue(p.TenantId.Value, out var tn) ? tn : null,
                OwnerName = p.OwnerId.HasValue && ownerNames.TryGetValue(p.OwnerId.Value, out var on) ? on : null
            }).ToList();

            var debts = debtsRaw.Select(d => new DashboardDebtDto
            {
                Id = d.Id,
                Amount = d.Amount,
                RemainingAmount = d.RemainingAmount,
                Type = (int)d.Type,
                Status = d.Status.ToString(),
                DueDate = d.DueDate,
                TenantId = d.TenantId,
                OwnerId = d.OwnerId,
                PeriodMonth = d.PeriodMonth,
                PeriodYear = d.PeriodYear,
                DisplayName = d.TenantId.HasValue && tenantNames.TryGetValue(d.TenantId.Value, out var tn2) ? tn2
                            : d.OwnerId.HasValue && ownerNames.TryGetValue(d.OwnerId.Value, out var on2) ? on2 + " (M)"
                            : "Bilinmiyor"
            }).ToList();

            return Result<DashboardSummaryDto>.Success(new DashboardSummaryDto
            {
                Payments = payments,
                Expenses = expenses,
                Debts = debts
            });
        }
        catch (Exception ex)
        {
            return Result<DashboardSummaryDto>.Failure($"Dashboard özeti alınamadı: {ex.Message}");
        }
    }

    private static (DateTime? start, DateTime? end) GetDateRange(string? filter)
    {
        var now = DateTime.UtcNow;

        if (int.TryParse(filter, out int year) && year > 2000 && year < 2100)
        {
            return (new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc));
        }

        return filter switch
        {
            "this_month" => (new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                             new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, DateTimeKind.Utc)),
            "last_month" => (new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                             new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(-1)),
            "this_year" => (new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            new DateTime(now.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc)),
            _ => (null, null)
        };
    }
}

public class DashboardSummaryDto
{
    public List<DashboardPaymentDto> Payments { get; set; } = new();
    public List<DashboardExpenseDto> Expenses { get; set; } = new();
    public List<DashboardDebtDto> Debts { get; set; } = new();
}

public class DashboardPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
    public string? TenantName { get; set; }
    public string? OwnerName { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
}

public class DashboardExpenseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
}

public class DashboardDebtDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int Type { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public string? DisplayName { get; set; }
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
}
