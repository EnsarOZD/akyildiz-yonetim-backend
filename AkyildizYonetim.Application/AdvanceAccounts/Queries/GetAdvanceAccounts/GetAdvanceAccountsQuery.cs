using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using AkyildizYonetim.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccounts;

public record GetAdvanceAccountsQuery : IRequest<Result<PagedResult<AdvanceAccountDto>>>
{
    public Guid? TenantId { get; init; }
    public bool? ActiveOnly { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAdvanceAccountsQueryHandler : IRequestHandler<GetAdvanceAccountsQuery, Result<PagedResult<AdvanceAccountDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAdvanceAccountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<AdvanceAccountDto>>> Handle(GetAdvanceAccountsQuery request, CancellationToken cancellationToken)
    {
        // Data Scope Resolution
        var fullAccessRoles = new Func<ICurrentUserService, bool>[] { 
            u => u.IsAdmin, u => u.IsManager, u => u.IsDataEntry, u => u.IsObserver 
        };

        var effectiveTenantId = DataScopeHelper.ResolveTenantId(_currentUserService, request.TenantId, fullAccessRoles);

        if (DataScopeHelper.IsScopeRestricted(_currentUserService, fullAccessRoles))
        {
            if (!effectiveTenantId.HasValue)
            {
                return Result<PagedResult<AdvanceAccountDto>>.Success(new PagedResult<AdvanceAccountDto>());
            }
        }

        var query = _context.AdvanceAccounts
            .AsNoTracking()
            .AsQueryable();

        if (effectiveTenantId.HasValue)
        {
            query = query.Where(aa => aa.TenantId == effectiveTenantId.Value);
        }

        if (request.ActiveOnly == true)
        {
            query = query.Where(aa => aa.IsActive && aa.Balance > 0);
        }

        var pagedResult = await query
            .OrderByDescending(aa => aa.CreatedAt)
            .ThenByDescending(aa => aa.Id)
            .Select(aa => new AdvanceAccountDto
            {
                Id = aa.Id,
                TenantId = aa.TenantId,
                TenantName = aa.Tenant.CompanyName ?? "Bilinmiyor",
                Balance = aa.Balance,
                Description = aa.Description,
                IsActive = aa.IsActive,
                CreatedAt = aa.CreatedAt,
                UpdatedAt = aa.UpdatedAt ?? aa.CreatedAt
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result<PagedResult<AdvanceAccountDto>>.Success(pagedResult);
    }
}
 