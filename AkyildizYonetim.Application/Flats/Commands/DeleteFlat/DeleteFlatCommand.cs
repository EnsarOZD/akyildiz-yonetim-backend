using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Flats.Commands.DeleteFlat;

public record DeleteFlatCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteFlatCommandHandler : IRequestHandler<DeleteFlatCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteFlatCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result> Handle(DeleteFlatCommand request, CancellationToken cancellationToken)
    {
        var flat = await _context.Flats.FirstOrDefaultAsync(f => f.Id == request.Id && !f.IsDeleted, cancellationToken);
        if (flat == null)
            return Result.Failure("Daire bulunamadı.");
        flat.IsDeleted = true;
        flat.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 