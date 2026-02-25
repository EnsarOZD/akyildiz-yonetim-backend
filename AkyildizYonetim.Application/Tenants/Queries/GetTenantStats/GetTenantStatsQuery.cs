using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Queries.GetTenantStats;

public record GetTenantStatsQuery : IRequest<Result<TenantStatsDto>>
{
}

public class GetTenantStatsQueryHandler : IRequestHandler<GetTenantStatsQuery, Result<TenantStatsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTenantStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TenantStatsDto>> Handle(GetTenantStatsQuery request, CancellationToken cancellationToken)
    {
        var tenantsQuery = _context.Tenants.Where(t => !t.IsDeleted);
        var flatsQuery = _context.Flats.Where(f => !f.IsDeleted);
        var debtsQuery = _context.UtilityDebts.Where(d => !d.IsDeleted);
        var advanceQuery = _context.AdvanceAccounts.Where(a => !a.IsDeleted && a.IsActive);

        if (!_currentUserService.IsAdmin && !_currentUserService.IsManager)
        {
            if (_currentUserService.TenantId.HasValue)
            {
                var tid = _currentUserService.TenantId.Value;
                tenantsQuery = tenantsQuery.Where(t => t.Id == tid);
                flatsQuery = flatsQuery.Where(f => f.TenantId == tid);
                debtsQuery = debtsQuery.Where(d => d.TenantId == tid);
                advanceQuery = advanceQuery.Where(a => a.TenantId == tid);
            }
            else if (_currentUserService.OwnerId.HasValue)
            {
                var oid = _currentUserService.OwnerId.Value;
                tenantsQuery = tenantsQuery.Where(t => t.Flats.Any(f => f.OwnerId == oid));
                flatsQuery = flatsQuery.Where(f => f.OwnerId == oid);
                debtsQuery = debtsQuery.Where(d => d.OwnerId == oid);
                advanceQuery = advanceQuery.Where(a => _context.Flats.Any(f => f.OwnerId == oid && f.TenantId == a.TenantId));
            }
            else
            {
                return Result<TenantStatsDto>.Success(new TenantStatsDto());
            }
        }

        var totalTenants = await tenantsQuery.CountAsync(cancellationToken);
        var activeTenants = await tenantsQuery.Where(t => t.IsActive).CountAsync(cancellationToken);
        
        var totalFlats = await flatsQuery.CountAsync(cancellationToken);
        var occupiedFlats = await flatsQuery.Where(f => f.IsOccupied).CountAsync(cancellationToken);
        
        var totalDebt = await debtsQuery
            .Where(d => d.Status != Domain.Entities.DebtStatus.Paid)
            .SumAsync(d => d.RemainingAmount, cancellationToken) -
                        await advanceQuery
            .SumAsync(a => a.Balance, cancellationToken);

        var occupancyRate = totalFlats > 0 
            ? Math.Round((double)occupiedFlats / totalFlats * 100, 1) 
            : 0;

        var stats = new TenantStatsDto
        {
            TotalCount = totalTenants,
            ActiveCount = activeTenants,
            PassiveCount = totalTenants - activeTenants,
            TotalFlats = totalFlats,
            OccupiedFlats = occupiedFlats,
            OccupancyRate = occupancyRate,
            TotalDebt = totalDebt
        };

        return Result<TenantStatsDto>.Success(stats);
    }
}

public class TenantStatsDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PassiveCount { get; set; }
    public int TotalFlats { get; set; }
    public int OccupiedFlats { get; set; }
    public double OccupancyRate { get; set; }
    public decimal TotalDebt { get; set; }
} 