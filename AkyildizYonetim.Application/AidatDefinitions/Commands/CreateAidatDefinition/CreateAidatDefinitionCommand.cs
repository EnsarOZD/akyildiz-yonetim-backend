using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.AidatDefinitions.Commands.CreateAidatDefinition;

public record CreateAidatDefinitionCommand : IRequest<Result<Guid>>
{
    public Guid TenantId { get; init; }
    public string Unit { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal Amount { get; init; }
    public decimal VatIncludedAmount { get; init; }
}

public class CreateAidatDefinitionCommandHandler : IRequestHandler<CreateAidatDefinitionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateAidatDefinitionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateAidatDefinitionCommand request, CancellationToken cancellationToken)
    {
        var aidatDefinition = new AidatDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Unit = request.Unit,
            Year = request.Year,
            Amount = request.Amount,
            VatIncludedAmount = request.VatIncludedAmount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.AidatDefinitions.Add(aidatDefinition);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(aidatDefinition.Id);
    }
} 