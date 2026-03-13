using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AidatDefinitions.Queries.GetAidatDefinitions;

public record GetAidatDefinitionsQuery : IRequest<Result<PagedResult<AidatDefinitionDto>>>
{
    public Guid? TenantId { get; init; }
    public int? Year { get; init; }
    public string? Unit { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAidatDefinitionsQueryHandler : IRequestHandler<GetAidatDefinitionsQuery, Result<PagedResult<AidatDefinitionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAidatDefinitionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<AidatDefinitionDto>>> Handle(GetAidatDefinitionsQuery request, CancellationToken cancellationToken)
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
                return Result<PagedResult<AidatDefinitionDto>>.Success(new PagedResult<AidatDefinitionDto>());
            }
        }

        var query = _context.AidatDefinitions
            .AsNoTracking()
            .Include(ad => ad.Tenant)
            .AsQueryable();

        if (effectiveTenantId.HasValue)
            query = query.Where(ad => ad.TenantId == effectiveTenantId.Value);

        if (request.Year.HasValue)
            query = query.Where(ad => ad.Year == request.Year.Value);

        if (!string.IsNullOrEmpty(request.Unit))
            query = query.Where(ad => ad.Unit.Contains(request.Unit));

        if (request.IsActive.HasValue)
            query = query.Where(ad => ad.IsActive == request.IsActive.Value);

        var pagedResult = await query
            .OrderByDescending(ad => ad.Year)
            .ThenBy(ad => ad.Unit)
            .ThenBy(ad => ad.Id) 
            .Select(ad => new AidatDefinitionDto
            {
                Id = ad.Id,
                TenantId = ad.TenantId,
                TenantName = ad.Tenant.CompanyName ?? "Bilinmiyor",
                Unit = ad.Unit,
                Year = ad.Year,
                Amount = ad.Amount,
                VatIncludedAmount = ad.VatIncludedAmount,
                IsActive = ad.IsActive,
                CreatedAt = ad.CreatedAt,
                UpdatedAt = ad.UpdatedAt
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
 
        return Result<PagedResult<AidatDefinitionDto>>.Success(pagedResult);
    }
}

public class AidatDefinitionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public decimal VatIncludedAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 