using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.Flats.Commands.CreateFlat;

public record CreateFlatCommand : IRequest<Result<Guid>>
{
    public string Number { get; init; } = string.Empty;
    public string UnitNumber { get; init; } = string.Empty;
    public int Floor { get; init; }
    public decimal UnitArea { get; init; }
    public int RoomCount { get; init; }
    public string ApartmentNumber { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string Category { get; init; } = "Normal";
    public int ShareCount { get; init; } = 1;
    public string BusinessType { get; init; } = string.Empty;
    public decimal MonthlyRent { get; init; } = 0;
    public string Description { get; init; } = string.Empty;
}

public class CreateFlatCommandHandler : IRequestHandler<CreateFlatCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateFlatCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<Guid>> Handle(CreateFlatCommand request, CancellationToken cancellationToken)
    {
        var flat = new Flat
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
            UnitNumber = request.UnitNumber,
            Floor = request.Floor,
            UnitArea = request.UnitArea,
            RoomCount = request.RoomCount,
            ApartmentNumber = request.ApartmentNumber,
            OwnerId = request.OwnerId,
            Category = request.Category,
            ShareCount = request.ShareCount,
            BusinessType = request.BusinessType,
            MonthlyRent = request.MonthlyRent,
            Description = request.Description,
            IsActive = true,
            IsOccupied = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Flats.Add(flat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(flat.Id);
    }
} 