using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Queries.GetTenants;

public record GetTenantsQuery : IRequest<Result<List<TenantDto>>>
{
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? Period { get; init; }
    public bool? ShowOnlyOccupied { get; init; }
    public int? Floor { get; init; }
    public string? Category { get; init; }
}

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, Result<List<TenantDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TenantDto>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenants.Where(t => !t.IsDeleted).AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(t => t.IsActive == request.IsActive.Value);

        if (request.Period.HasValue)
        {
            var period = request.Period.Value;
            query = query.Where(t =>
                t.ContractStartDate <= period &&
                (t.ContractEndDate == null || t.ContractEndDate >= period)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.CompanyName.ToLower().Contains(searchTerm) ||
                t.ContactPersonName.ToLower().Contains(searchTerm) ||
                t.TaxNumber.Contains(searchTerm) ||
                t.ContactPersonEmail.ToLower().Contains(searchTerm) ||
                t.ContactPersonPhone.Contains(searchTerm));
        }

        if (request.ShowOnlyOccupied.HasValue)
        {
            if (request.ShowOnlyOccupied.Value)
            {
                // Sadece dolu üniteleri olan kiracıları getir
                query = query.Where(t => t.Flats.Any(f => f.IsOccupied));
            }
            else
            {
                // Sadece boş üniteleri olan kiracıları getir (bu durumda kiracı olmaz)
                query = query.Where(t => !t.Flats.Any(f => f.IsOccupied));
            }
        }

        if (request.Floor.HasValue)
        {
            query = query.Where(t => t.Flats.Any(f => f.Floor == request.Floor.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(t => t.Flats.Any(f => f.Category == request.Category));
        }

        var tenants = await query
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
            .ToListAsync(cancellationToken);

        return Result<List<TenantDto>>.Success(tenants);
    }
}

// Boş üniteleri getiren ayrı query
public record GetAvailableFlatsQuery : IRequest<Result<List<FlatInfoDto>>>
{
    public int? Floor { get; init; }
    public string? Category { get; init; }
    public string? SearchTerm { get; init; }
}

public class GetAvailableFlatsQueryHandler : IRequestHandler<GetAvailableFlatsQuery, Result<List<FlatInfoDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableFlatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FlatInfoDto>>> Handle(GetAvailableFlatsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Flats.Where(f => !f.IsDeleted && !f.IsOccupied && f.IsActive).AsQueryable();

        if (request.Floor.HasValue)
            query = query.Where(f => f.Floor == request.Floor.Value);

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(f => f.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(f => 
                f.UnitNumber.ToLower().Contains(searchTerm) ||
                f.Number.ToLower().Contains(searchTerm));
        }

        var availableFlats = await query
            .OrderBy(f => f.Floor)
            .ThenBy(f => f.UnitNumber)
            .Select(f => new FlatInfoDto
            {
                Id = f.Id,
                UnitNumber = f.UnitNumber,
                Floor = f.Floor,
                UnitArea = f.UnitArea,
                Category = f.Category,
                IsOccupied = f.IsOccupied
            })
            .ToListAsync(cancellationToken);

        return Result<List<FlatInfoDto>>.Success(availableFlats);
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