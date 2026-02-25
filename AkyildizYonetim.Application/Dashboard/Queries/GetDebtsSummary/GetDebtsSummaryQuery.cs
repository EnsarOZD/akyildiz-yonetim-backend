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
            // 1. Get all active unpaid utility debts
            var debts = await _context.UtilityDebts
                .Where(d => !d.IsDeleted && d.RemainingAmount > 0)
                .Include(d => d.Flat)
                .ToListAsync(cancellationToken);

            // 2. Group by TenantId (preferred) or OwnerId
            var groupedDebts = debts
                .GroupBy(d => d.TenantId.HasValue ? d.TenantId.Value : (d.OwnerId.HasValue ? d.OwnerId.Value : Guid.Empty))
                .Where(g => g.Key != Guid.Empty)
                .ToList();

            var result = new List<DebtsSummaryDto>();

            foreach (var group in groupedDebts)
            {
                var id = group.Key;
                var firstDebt = group.First();
                
                string displayName = "Bilinmiyor";
                string entityType = "Unknown";
                if (firstDebt.TenantId.HasValue && firstDebt.TenantId.Value == id)
                {
                    var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
                    displayName = !string.IsNullOrEmpty(tenant?.CompanyName) ? tenant.CompanyName : (tenant?.ContactPersonName ?? "Bilinmeyen Kiracı");
                    entityType = "Tenant";
                }
                else if (firstDebt.OwnerId.HasValue && firstDebt.OwnerId.Value == id)
                {
                    var owner = await _context.Owners.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
                    displayName = owner != null ? $"{owner.FirstName} {owner.LastName}" : "Bilinmeyen Mal Sahibi";
                    entityType = "Owner";
                }

                // Collect all unique flat codes for this entity
                var apartmentNumber = string.Join(", ", group
                    .Select(d => d.Flat?.Code)
                    .Where(code => !string.IsNullOrEmpty(code))
                    .Distinct()
                    .OrderBy(code => code));

                var aidat = group.Where(d => d.Type == DebtType.Aidat).Sum(d => d.RemainingAmount);
                var electricity = group.Where(d => d.Type == DebtType.Electricity).Sum(d => d.RemainingAmount);
                var water = group.Where(d => d.Type == DebtType.Water).Sum(d => d.RemainingAmount);

                result.Add(new DebtsSummaryDto
                {
                    EntityId = id,
                    EntityType = entityType,
                    DisplayName = displayName,
                    ApartmentNumber = apartmentNumber,
                    AidatDebt = aidat,
                    ElectricityDebt = electricity,
                    WaterDebt = water,
                    TotalDebt = aidat + electricity + water
                });
            }

            return Result<List<DebtsSummaryDto>>.Success(result.OrderByDescending(x => x.TotalDebt).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<DebtsSummaryDto>>.Failure($"Borç özeti alınamadı: {ex.Message}");
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
