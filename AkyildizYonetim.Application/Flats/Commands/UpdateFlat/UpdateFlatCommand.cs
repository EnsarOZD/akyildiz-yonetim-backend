using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Flats.Commands.UpdateFlat;

public record UpdateFlatCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public int Floor { get; init; }
    public Guid OwnerId { get; init; }
    public Guid? TenantId { get; init; }
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
        flat.Floor = request.Floor;
        flat.OwnerId = request.OwnerId;
        flat.TenantId = request.TenantId;
        flat.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 