using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery : IRequest<Result<TenantDto>>
{
    public Guid Id { get; init; }
}

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    public GetTenantByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.Where(t => t.Id == request.Id && !t.IsDeleted)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                CompanyName = t.CompanyName,
                BusinessType = t.BusinessType,
                TaxNumber = t.TaxNumber,
                ContactPersonName = t.ContactPersonName,
                ContactPersonPhone = t.ContactPersonPhone,
                ContactPersonEmail = t.ContactPersonEmail,
                MonthlyAidat = t.MonthlyAidat,
                ElectricityRate = t.ElectricityRate,
                WaterRate = t.WaterRate,
                ContractStartDate = t.ContractStartDate,
                ContractEndDate = t.ContractEndDate,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Flats = _context.Flats
                    .Where(f => f.TenantId == t.Id && !f.IsDeleted)
                    .Select(f => new FlatInfoDto
                    {
                        Id = f.Id,
                        UnitNumber = f.UnitNumber,
                        Floor = f.Floor,
                        UnitArea = f.UnitArea,
                        Category = f.Category,
                        IsOccupied = f.IsOccupied
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (tenant == null)
            return Result<TenantDto>.Failure("Kiracı bulunamadı.");
        return Result<TenantDto>.Success(tenant);
    }
}

public class TenantDto
{
    public Guid Id { get; set; }
    
    // İş Yeri Bilgileri
    public string CompanyName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    
    // İletişim Kişisi Bilgileri
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public string ContactPersonEmail { get; set; } = string.Empty;
    
    // Aidat ve Borç Yönetimi
    public decimal MonthlyAidat { get; set; }
    public decimal ElectricityRate { get; set; }
    public decimal WaterRate { get; set; }
    
    // Sözleşme Bilgileri
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FlatInfoDto> Flats { get; set; } = new List<FlatInfoDto>();
}

public class FlatInfoDto
{
    public Guid Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal UnitArea { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
} 