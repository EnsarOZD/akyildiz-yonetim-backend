using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
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
                Floor = f.Floor,
                OwnerId = f.OwnerId,
                TenantId = f.TenantId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                FlatNumber = f.Number
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (flat == null)
            return Result<FlatDto>.Failure("Daire bulunamadı.");
        return Result<FlatDto>.Success(flat);
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