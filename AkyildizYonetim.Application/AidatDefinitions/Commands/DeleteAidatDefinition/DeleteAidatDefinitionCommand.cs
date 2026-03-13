using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AidatDefinitions.Commands.DeleteAidatDefinition;

public record DeleteAidatDefinitionCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteAidatDefinitionCommandHandler : IRequestHandler<DeleteAidatDefinitionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteAidatDefinitionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteAidatDefinitionCommand request, CancellationToken cancellationToken)
    {
        var aidatDefinition = await _context.AidatDefinitions
            .FirstOrDefaultAsync(ad => ad.Id == request.Id, cancellationToken);

        if (aidatDefinition == null)
            return Result.Failure("Aidat tanımı bulunamadı.");

        _context.AidatDefinitions.Remove(aidatDefinition);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 