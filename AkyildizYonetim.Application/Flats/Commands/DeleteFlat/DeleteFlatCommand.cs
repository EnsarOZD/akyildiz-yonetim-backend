using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.Flats.Commands.DeleteFlat;

public record DeleteFlatCommand : IRequest<Result>
{
	public Guid Id { get; init; }
}

public class DeleteFlatCommandHandler : IRequestHandler<DeleteFlatCommand, Result>
{
	private readonly IApplicationDbContext _context;
	public DeleteFlatCommandHandler(IApplicationDbContext context) => _context = context;

	public async Task<Result> Handle(DeleteFlatCommand request, CancellationToken ct)
{
    var flat = await _context.Flats
        .FirstOrDefaultAsync(f => f.Id == request.Id && !f.IsDeleted, ct);

    if (flat is null)
        return Result.Failure("Ünite bulunamadı.");

    // Dolu üniteyi silmeyi engelle (opsiyonel ama önerilir)
    if (flat.IsOccupied)
        return Result.Failure("Ünite doluyken silinemez. Önce kiracı ilişiğini kaldırın.");

    // Soft delete
    flat.IsDeleted = true;
    flat.IsActive  = false;
    flat.IsOccupied = false;
    flat.TenantId = null;          // ilişkiyi kopar
    flat.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync(ct);
    return Result.Success();
}
}
