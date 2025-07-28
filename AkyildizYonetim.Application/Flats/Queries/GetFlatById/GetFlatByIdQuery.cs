using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Flats.Queries.GetFlatById;

public record GetFlatByIdQuery : IRequest<Result<FlatDto>>
{
    public Guid Id { get; init; }
}

public class GetFlatByIdQueryHandler : IRequestHandler<GetFlatByIdQuery, Result<FlatDto>>
{
    private readonly IApplicationDbContext _context;
    public GetFlatByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<FlatDto>> Handle(GetFlatByIdQuery request, CancellationToken cancellationToken)
    {
        var flat = await _context.Flats.Where(f => f.Id == request.Id && !f.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);
        if (flat == null)
            return Result<FlatDto>.Failure("Daire bulunamadı.");
        return Result<FlatDto>.Success(flat);
    }
}

 