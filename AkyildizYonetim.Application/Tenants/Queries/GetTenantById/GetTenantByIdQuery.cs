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
                UpdatedAt = t.UpdatedAt
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
} 