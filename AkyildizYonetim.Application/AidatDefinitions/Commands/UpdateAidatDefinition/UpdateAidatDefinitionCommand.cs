using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AidatDefinitions.Commands.UpdateAidatDefinition;

public record UpdateAidatDefinitionCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Unit { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal Amount { get; init; }
    public decimal VatIncludedAmount { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateAidatDefinitionCommandHandler : IRequestHandler<UpdateAidatDefinitionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAidatDefinitionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAidatDefinitionCommand request, CancellationToken cancellationToken)
    {
        var aidatDefinition = await _context.AidatDefinitions
            .FirstOrDefaultAsync(ad => ad.Id == request.Id && !ad.IsDeleted, cancellationToken);

        if (aidatDefinition == null)
            return Result.Failure("Aidat tanımı bulunamadı.");

        aidatDefinition.TenantId = request.TenantId;
        aidatDefinition.Unit = request.Unit;
        aidatDefinition.Year = request.Year;
        aidatDefinition.Amount = request.Amount;
        aidatDefinition.VatIncludedAmount = request.VatIncludedAmount;
        aidatDefinition.IsActive = request.IsActive;
        aidatDefinition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
} 