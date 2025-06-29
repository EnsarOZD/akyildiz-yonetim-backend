using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.Flats.Commands.CreateFlat;

public record CreateFlatCommand : IRequest<Result<Guid>>
{
    public string Number { get; init; } = string.Empty;
    public int Floor { get; init; }
    public Guid OwnerId { get; init; }
    public Guid? TenantId { get; init; }
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
            Floor = request.Floor,
            OwnerId = request.OwnerId,
            TenantId = request.TenantId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Flats.Add(flat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(flat.Id);
    }
} 