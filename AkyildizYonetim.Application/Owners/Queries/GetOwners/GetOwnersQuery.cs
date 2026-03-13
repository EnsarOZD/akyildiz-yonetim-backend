using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Owners.Queries.GetOwners;

public record GetOwnersQuery : IRequest<Result<PagedResult<OwnerDto>>>
{
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetOwnersQueryHandler : IRequestHandler<GetOwnersQuery, Result<PagedResult<OwnerDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetOwnersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<OwnerDto>>> Handle(GetOwnersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Owners
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .AsQueryable();

        // Veri İzolasyonu (RBAC)
        if (!_currentUserService.IsAdmin && !_currentUserService.IsManager)
        {
            if (_currentUserService.OwnerId.HasValue)
            {
                query = query.Where(o => o.Id == _currentUserService.OwnerId.Value);
            }
            else if (_currentUserService.TenantId.HasValue)
            {
                // Kiracı, kendi dairesinin mal sahibini görebilir mi?
                query = query.Where(o => _context.Flats.Any(f => f.TenantId == _currentUserService.TenantId.Value && f.OwnerId == o.Id));
            }
            else
            {
                return Result<PagedResult<OwnerDto>>.Success(new PagedResult<OwnerDto>());
            }
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(o => o.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(o => 
                o.FirstName.ToLower().Contains(searchTerm) ||
                o.LastName.ToLower().Contains(searchTerm) ||
                o.ApartmentNumber.ToLower().Contains(searchTerm) ||
                o.Email.ToLower().Contains(searchTerm) ||
                o.PhoneNumber.Contains(searchTerm));
        }

        var pagedOwners = await query
            .OrderBy(o => o.FirstName)
            .ThenBy(o => o.LastName)
            .ThenBy(o => o.Id) // Stable sort
            .Select(o => new OwnerDto
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
                PhoneNumber = o.PhoneNumber,
                Email = o.Email,
                ApartmentNumber = o.ApartmentNumber,
                MonthlyDues = o.MonthlyDues,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Flats = _context.Flats
                    .Where(f => f.OwnerId == o.Id && !f.IsDeleted)
                    .Select(f => new FlatSummaryDto
                    {
                        Id = f.Id,
                        Code = f.Code
                    })
                    .ToList()
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result<PagedResult<OwnerDto>>.Success(pagedOwners);
    }
} 