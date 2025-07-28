using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Flats.Queries.GetFlats;

public record GetFlatsQuery : IRequest<Result<List<FlatDto>>>
{
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
    public string? Number { get; init; }
    public int? Floor { get; init; }
}

public class GetFlatsQueryHandler : IRequestHandler<GetFlatsQuery, Result<List<FlatDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetFlatsQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<List<FlatDto>>> Handle(GetFlatsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Flats.Where(f => !f.IsDeleted).AsQueryable();
        if (request.OwnerId.HasValue)
            query = query.Where(f => f.OwnerId == request.OwnerId.Value);
        if (request.TenantId.HasValue)
            query = query.Where(f => f.TenantId == request.TenantId.Value);
        if (!string.IsNullOrWhiteSpace(request.Number))
            query = query.Where(f => f.Number == request.Number);
        if (request.Floor.HasValue)
            query = query.Where(f => f.Floor == request.Floor.Value);
        var flats = await query.OrderBy(f => f.Floor).ThenBy(f => f.Number)
            .Select(f => new FlatDto
            {
                Id = f.Id,
                Number = f.Number,
                UnitNumber = f.UnitNumber,
                Floor = f.Floor,
                UnitArea = f.UnitArea,
                RoomCount = f.RoomCount,
                ApartmentNumber = f.ApartmentNumber,
                OwnerId = f.OwnerId,
                TenantId = f.TenantId,
                IsActive = f.IsActive,
                IsOccupied = f.IsOccupied,
                Category = f.Category,
                ShareCount = f.ShareCount,
                BusinessType = f.BusinessType,
                MonthlyRent = f.MonthlyRent,
                Description = f.Description,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync(cancellationToken);
        return Result<List<FlatDto>>.Success(flats);
    }
}

 