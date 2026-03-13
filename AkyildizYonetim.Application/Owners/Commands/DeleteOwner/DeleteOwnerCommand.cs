using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Owners.Commands.DeleteOwner;

public record DeleteOwnerCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteOwnerCommandHandler : IRequestHandler<DeleteOwnerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteOwnerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (owner == null)
        {
            return Result.Failure("Ev sahibi bulunamadı.");
        }

        _context.Owners.Remove(owner);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 