using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Flats.Commands.UpdateFlat;

public record UpdateFlatCommand : IRequest<Result>
{
    public Guid Id { get; init; }
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
    public bool IsActive { get; init; } = true;
    public bool IsOccupied { get; init; } = false;
}

public class UpdateFlatCommandHandler : IRequestHandler<UpdateFlatCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateFlatCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result> Handle(UpdateFlatCommand request, CancellationToken cancellationToken)
    {
        var flat = await _context.Flats.FirstOrDefaultAsync(f => f.Id == request.Id && !f.IsDeleted, cancellationToken);
        if (flat == null)
            return Result.Failure("Daire bulunamadı.");
        flat.Number = request.Number;
        flat.UnitNumber = request.UnitNumber;
        flat.Floor = request.Floor;
        flat.UnitArea = request.UnitArea;
        flat.RoomCount = request.RoomCount;
        flat.ApartmentNumber = request.ApartmentNumber;
        flat.OwnerId = request.OwnerId;
        flat.Category = request.Category;
        flat.ShareCount = request.ShareCount;
        flat.BusinessType = request.BusinessType;
        flat.MonthlyRent = request.MonthlyRent;
        flat.Description = request.Description;
        flat.IsActive = request.IsActive;
        flat.IsOccupied = request.IsOccupied;
        flat.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 