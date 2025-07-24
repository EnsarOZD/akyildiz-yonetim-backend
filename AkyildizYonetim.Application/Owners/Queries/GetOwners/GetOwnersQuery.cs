using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Owners.Queries.GetOwners;

public record GetOwnersQuery : IRequest<Result<List<OwnerDto>>>
{
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}

public class GetOwnersQueryHandler : IRequestHandler<GetOwnersQuery, Result<List<OwnerDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetOwnersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<OwnerDto>>> Handle(GetOwnersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Owners
            .Where(o => !o.IsDeleted)
            .AsQueryable();

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

        var owners = await query
            .OrderBy(o => o.FirstName)
            .ThenBy(o => o.LastName)
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
                    .Select(f => f.Id)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Result<List<OwnerDto>>.Success(owners);
    }
} 