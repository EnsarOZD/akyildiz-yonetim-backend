using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.Dashboard.Queries.GetDebtsSummary;

public record GetDebtsSummaryQuery(int? Year = null) : IRequest<Result<List<DebtsSummaryDto>>>;

public class GetDebtsSummaryQueryHandler
    : IRequestHandler<GetDebtsSummaryQuery, Result<List<DebtsSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDebtsSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DebtsSummaryDto>>> Handle(
        GetDebtsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Join Flat separately to avoid nav-property + global soft-delete filter translation issue
            var debtsRaw = _context.UtilityDebts
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(d => !d.IsDeleted && d.RemainingAmount > 0);

            if (request.Year.HasValue)
            {
                var startDate = new DateTime(request.Year.Value, 1, 1);
                var endDate = new DateTime(request.Year.Value, 12, 31, 23, 59, 59);
                debtsRaw = debtsRaw.Where(d => d.DueDate >= startDate && d.DueDate <= endDate);
            }

            var debts = await debtsRaw
                .Join(
                    _context.Flats.AsNoTracking().IgnoreQueryFilters(),
                    d => d.FlatId,
                    f => f.Id,
                    (d, f) => new
                    {
                        d.TenantId,
                        d.OwnerId,
                        d.Type,
                        d.RemainingAmount,
                        d.DueDate,
                        FlatCode = f.Code,
                        FlatNumber = f.Number
                    })
                .ToListAsync(cancellationToken);

            // Group in memory — avoids EF Core SQL translation limitations
            var groups = debts
                .Where(d => d.TenantId.HasValue || d.OwnerId.HasValue)
                .GroupBy(d => d.TenantId ?? d.OwnerId)
                .Where(g => g.Key.HasValue)
                .Select(g => new
                {
                    EntityId = g.Key!.Value,
                    IsTenant = g.Any(d => d.TenantId.HasValue),
                    FlatCodes = g.Select(d => !string.IsNullOrEmpty(d.FlatCode) ? d.FlatCode : d.FlatNumber).Distinct().OrderBy(c => c).ToList(),
                    AidatDebt = g.Where(d => d.Type == DebtType.Aidat).Sum(d => d.RemainingAmount),
                    ElectricityDebt = g.Where(d => d.Type == DebtType.Electricity).Sum(d => d.RemainingAmount),
                    WaterDebt = g.Where(d => d.Type == DebtType.Water).Sum(d => d.RemainingAmount)
                })
                .ToList();

            // Fetch display names in two targeted queries
            var tenantIds = groups.Where(x => x.IsTenant).Select(x => x.EntityId).ToList();
            var ownerIds = groups.Where(x => !x.IsTenant).Select(x => x.EntityId).ToList();

            var tenants = tenantIds.Count > 0
                ? (await _context.Tenants
                    .AsNoTracking()
                    .Select(t => new { t.Id, Name = !string.IsNullOrEmpty(t.CompanyName) ? t.CompanyName : t.ContactPersonName })
                    .ToListAsync(cancellationToken))
                    .Where(t => tenantIds.Contains(t.Id))
                    .ToDictionary(t => t.Id, t => t.Name)
                : new Dictionary<Guid, string?>();

            var owners = ownerIds.Count > 0
                ? (await _context.Owners
                    .AsNoTracking()
                    .Select(o => new { o.Id, Name = o.FirstName + " " + o.LastName })
                    .ToListAsync(cancellationToken))
                    .Where(o => ownerIds.Contains(o.Id))
                    .ToDictionary(o => o.Id, o => o.Name)
                : new Dictionary<Guid, string>();

            var finalResult = groups.Select(x => new DebtsSummaryDto
            {
                EntityId = x.EntityId,
                EntityType = x.IsTenant ? "Tenant" : "Owner",
                DisplayName = x.IsTenant
                    ? (tenants.TryGetValue(x.EntityId, out var tName) ? tName ?? "Bilinmeyen Kiracı" : "Bilinmeyen Kiracı")
                    : (owners.TryGetValue(x.EntityId, out var oName) ? oName : "Bilinmeyen Mal Sahibi"),
                ApartmentNumber = string.Join(", ", x.FlatCodes.Where(f => f != null)),
                AidatDebt = x.AidatDebt,
                ElectricityDebt = x.ElectricityDebt,
                WaterDebt = x.WaterDebt,
                TotalDebt = x.AidatDebt + x.ElectricityDebt + x.WaterDebt
            })
            .OrderByDescending(x => x.TotalDebt)
            .ToList();

            return Result<List<DebtsSummaryDto>>.Success(finalResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetDebtsSummaryQuery: {ex}");
            return Result<List<DebtsSummaryDto>>.Failure($"Borç özeti alınamadı: {ex.Message} {ex.InnerException?.Message}");
        }
    }
}

public class DebtsSummaryDto
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = null!; // "Tenant" | "Owner"
    public string DisplayName { get; set; } = null!;
    public string? ApartmentNumber { get; set; }
    public decimal AidatDebt { get; set; }
    public decimal ElectricityDebt { get; set; }
    public decimal WaterDebt { get; set; }
    public decimal TotalDebt { get; set; }
}
