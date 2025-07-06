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
                t.LeaseStartDate <= period &&
                (t.LeaseEndDate == null || t.LeaseEndDate >= period)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.FirstName.ToLower().Contains(searchTerm) ||
                t.LastName.ToLower().Contains(searchTerm) ||
                t.ApartmentNumber.ToLower().Contains(searchTerm) ||
                t.Email.ToLower().Contains(searchTerm) ||
                t.PhoneNumber.Contains(searchTerm));
        }

        var tenants = await query.OrderBy(t => t.FirstName).ThenBy(t => t.LastName)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                PhoneNumber = t.PhoneNumber,
                Email = t.Email,
                ApartmentNumber = t.ApartmentNumber,
                LeaseStartDate = t.LeaseStartDate,
                LeaseEndDate = t.LeaseEndDate,
                MonthlyRent = t.MonthlyRent,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Flats = _context.Flats
                    .Where(f => f.TenantId == t.Id && !f.IsDeleted)
                    .Select(f => f.Id)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Result<List<TenantDto>>.Success(tenants);
    }
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Guid> Flats { get; set; } = new List<Guid>();
} 