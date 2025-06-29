using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
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
                Floor = f.Floor,
                OwnerId = f.OwnerId,
                TenantId = f.TenantId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                FlatNumber = f.Number
            })
            .ToListAsync(cancellationToken);
        return Result<List<FlatDto>>.Success(flats);
    }
}

public class FlatDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string FlatNumber { get; set; } = string.Empty;
} 