using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AidatDefinitions.Queries.GetAidatDefinitions;

public record GetAidatDefinitionsQuery : IRequest<Result<List<AidatDefinitionDto>>>
{
    public Guid? TenantId { get; init; }
    public int? Year { get; init; }
    public string? Unit { get; init; }
    public bool? IsActive { get; init; }
}

public class GetAidatDefinitionsQueryHandler : IRequestHandler<GetAidatDefinitionsQuery, Result<List<AidatDefinitionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAidatDefinitionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AidatDefinitionDto>>> Handle(GetAidatDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AidatDefinitions
            .Include(ad => ad.Tenant)
            .AsQueryable();

        if (request.TenantId.HasValue)
            query = query.Where(ad => ad.TenantId == request.TenantId.Value);

        if (request.Year.HasValue)
            query = query.Where(ad => ad.Year == request.Year.Value);

        if (!string.IsNullOrEmpty(request.Unit))
            query = query.Where(ad => ad.Unit.Contains(request.Unit));

        if (request.IsActive.HasValue)
            query = query.Where(ad => ad.IsActive == request.IsActive.Value);

        var aidatDefinitions = await query
            .OrderByDescending(ad => ad.Year)
            .ThenBy(ad => ad.Unit)
            .ToListAsync(cancellationToken);

        var dtos = aidatDefinitions.Select(ad => new AidatDefinitionDto
        {
            Id = ad.Id,
            TenantId = ad.TenantId,
            TenantName = $"{ad.Tenant.FirstName} {ad.Tenant.LastName}",
            Unit = ad.Unit,
            Year = ad.Year,
            Amount = ad.Amount,
            VatIncludedAmount = ad.VatIncludedAmount,
            IsActive = ad.IsActive,
            CreatedAt = ad.CreatedAt,
            UpdatedAt = ad.UpdatedAt
        }).ToList();

        return Result<List<AidatDefinitionDto>>.Success(dtos);
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