using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.ServiceRequests.Queries.GetServiceRequests;

public record GetServiceRequestsQuery(string? Status = null) : IRequest<Result<List<ServiceRequestDto>>>;

public class GetServiceRequestsQueryHandler : IRequestHandler<GetServiceRequestsQuery, Result<List<ServiceRequestDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetServiceRequestsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ServiceRequestDto>>> Handle(GetServiceRequestsQuery request, CancellationToken ct)
    {
        try
        {
            var query = _context.ServiceRequests.AsNoTracking().AsQueryable();

            // Kiracı sadece kendi taleplerini görür
            if (_currentUser.TenantId.HasValue && !_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsDataEntry)
                query = query.Where(sr => sr.TenantId == _currentUser.TenantId.Value);
            else if (_currentUser.OwnerId.HasValue && !_currentUser.IsAdmin && !_currentUser.IsManager)
                query = query.Where(sr => sr.OwnerId == _currentUser.OwnerId.Value);

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<ServiceRequestStatus>(request.Status, true, out var parsedStatus))
                query = query.Where(sr => sr.Status == parsedStatus);

            var items = await query
                .OrderByDescending(sr => sr.CreatedAt)
                .Select(sr => new ServiceRequestDto
                {
                    Id = sr.Id,
                    Title = sr.Title,
                    Description = sr.Description,
                    Status = sr.Status.ToString(),
                    Category = sr.Category.ToString(),
                    AdminNote = sr.AdminNote,
                    ClosedAt = sr.ClosedAt,
                    CreatedAt = sr.CreatedAt,
                    TenantId = sr.TenantId,
                    OwnerId = sr.OwnerId,
                    TenantName = sr.Tenant != null
                        ? (!string.IsNullOrEmpty(sr.Tenant.CompanyName) ? sr.Tenant.CompanyName : sr.Tenant.ContactPersonName)
                        : null,
                    OwnerName = sr.Owner != null ? sr.Owner.FirstName + " " + sr.Owner.LastName : null
                })
                .ToListAsync(ct);

            return Result<List<ServiceRequestDto>>.Success(items);
        }
        catch (Exception ex)
        {
            return Result<List<ServiceRequestDto>>.Failure($"Talepler alınamadı: {ex.Message}");
        }
    }
}

public class ServiceRequestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public string? TenantName { get; set; }
    public string? OwnerName { get; set; }
}
