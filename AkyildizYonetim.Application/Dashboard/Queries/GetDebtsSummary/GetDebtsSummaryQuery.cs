using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.Dashboard.Queries.GetDebtsSummary;

public record GetDebtsSummaryQuery : IRequest<Result<List<DebtsSummaryDto>>>;

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
            // 1. Get all active unpaid utility debts with grouping and projection to avoid N+1
            // We group by the entity that owes the debt (Tenant or Owner)
            var debtsQuery = _context.UtilityDebts
                .AsNoTracking()
                .Where(d => !d.IsDeleted && d.RemainingAmount > 0);

            // Fetch tenants and owners separately to avoid complex joins in a single query if preferred, 
            // but a projection with subqueries for names is also efficient in EF Core since it generates a single SQL.
            
            var result = await debtsQuery
                .GroupBy(d => d.TenantId ?? d.OwnerId)
                .Where(g => g.Key != null)
                .Select(g => new
                {
                    EntityId = g.Key!.Value,
                    IsTenant = g.Any(d => d.TenantId == g.Key),
                    FlatCodes = g.Select(d => d.Flat != null ? d.Flat.Code : "Bilinmiyor").Distinct(),
                    AidatDebt = g.Where(d => d.Type == DebtType.Aidat).Sum(d => (decimal?)d.RemainingAmount) ?? 0,
                    ElectricityDebt = g.Where(d => d.Type == DebtType.Electricity).Sum(d => (decimal?)d.RemainingAmount) ?? 0,
                    WaterDebt = g.Where(d => d.Type == DebtType.Water).Sum(d => (decimal?)d.RemainingAmount) ?? 0
                })
                .ToListAsync(cancellationToken);

            // Now we need the display names. Since we might have many entities, we'll fetch them in batches.
            var tenantIds = result.Where(x => x.IsTenant).Select(x => x.EntityId).ToList();
            var ownerIds = result.Where(x => !x.IsTenant).Select(x => x.EntityId).ToList();

            var tenants = await _context.Tenants
                .AsNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .Select(t => new { t.Id, Name = !string.IsNullOrEmpty(t.CompanyName) ? t.CompanyName : t.ContactPersonName })
                .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

            var owners = await _context.Owners
                .AsNoTracking()
                .Where(o => ownerIds.Contains(o.Id))
                .Select(o => new { o.Id, Name = o.FirstName + " " + o.LastName })
                .ToDictionaryAsync(o => o.Id, o => o.Name, cancellationToken);

            var finalResult = result.Select(x => new DebtsSummaryDto
            {
                EntityId = x.EntityId,
                EntityType = x.IsTenant ? "Tenant" : "Owner",
                DisplayName = x.IsTenant 
                    ? (tenants.TryGetValue(x.EntityId, out var tName) ? tName : "Bilinmeyen Kiracı")
                    : (owners.TryGetValue(x.EntityId, out var oName) ? oName : "Bilinmeyen Mal Sahibi"),
                ApartmentNumber = string.Join(", ", x.FlatCodes.Where(f => f != null).OrderBy(c => c)),
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
            // Log the detailed error for backend monitoring
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
