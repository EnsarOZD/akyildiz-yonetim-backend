using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AidatDefinitions.Queries.GetAidatDefinitionById;

public record GetAidatDefinitionByIdQuery : IRequest<Result<AidatDefinitionDto>>
{
    public Guid Id { get; init; }
}

public class GetAidatDefinitionByIdQueryHandler : IRequestHandler<GetAidatDefinitionByIdQuery, Result<AidatDefinitionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAidatDefinitionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AidatDefinitionDto>> Handle(GetAidatDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var aidatDefinition = await _context.AidatDefinitions
            .Include(ad => ad.Tenant)
            .FirstOrDefaultAsync(ad => ad.Id == request.Id && !ad.IsDeleted, cancellationToken);

        if (aidatDefinition == null)
            return Result<AidatDefinitionDto>.Failure("Aidat tanımı bulunamadı.");

        var dto = new AidatDefinitionDto
        {
            Id = aidatDefinition.Id,
            TenantId = aidatDefinition.TenantId,
            TenantName = $"{aidatDefinition.Tenant.FirstName} {aidatDefinition.Tenant.LastName}",
            Unit = aidatDefinition.Unit,
            Year = aidatDefinition.Year,
            Amount = aidatDefinition.Amount,
            VatIncludedAmount = aidatDefinition.VatIncludedAmount,
            IsActive = aidatDefinition.IsActive,
            CreatedAt = aidatDefinition.CreatedAt,
            UpdatedAt = aidatDefinition.UpdatedAt
        };

        return Result<AidatDefinitionDto>.Success(dto);
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