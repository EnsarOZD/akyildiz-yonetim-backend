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

    public GetTenantStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TenantStatsDto>> Handle(GetTenantStatsQuery request, CancellationToken cancellationToken)
    {
        var totalTenants = await _context.Tenants.Where(t => !t.IsDeleted).CountAsync(cancellationToken);
        var activeTenants = await _context.Tenants.Where(t => !t.IsDeleted && t.IsActive).CountAsync(cancellationToken);
        var passiveTenants = totalTenants - activeTenants;

        var stats = new TenantStatsDto
        {
            TotalCount = totalTenants,
            ActiveCount = activeTenants,
            PassiveCount = passiveTenants
        };

        return Result<TenantStatsDto>.Success(stats);
    }
}

public class TenantStatsDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PassiveCount { get; set; }
} 