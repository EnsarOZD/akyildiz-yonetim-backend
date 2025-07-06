using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands;

public record DistributeSharedConsumptionCommand : IRequest<List<DistributedConsumptionResult>>
{
    public decimal SharedAreaConsumption { get; init; }
    public decimal MescitConsumption { get; init; }
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
}

public class DistributedConsumptionResult
{
    public Guid FlatId { get; set; }
    public string FlatNumber { get; set; } = string.Empty;
    public int ShareCount { get; set; }
    public decimal DistributedConsumption { get; set; }
}

public class DistributeSharedConsumptionCommandHandler : IRequestHandler<DistributeSharedConsumptionCommand, List<DistributedConsumptionResult>>
{
    private readonly IApplicationDbContext _context;
    public DistributeSharedConsumptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DistributedConsumptionResult>> Handle(DistributeSharedConsumptionCommand request, CancellationToken cancellationToken)
    {
        // Sadece aktif ve Category'si 'Normal' olan katlar
        var flats = await _context.Flats
            .Where(f => f.IsActive && f.Category == "Normal" && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalShare = flats.Sum(f => f.ShareCount);
        if (totalShare == 0) return new List<DistributedConsumptionResult>();

        var totalShared = request.SharedAreaConsumption + request.MescitConsumption;
        var results = flats.Select(f => new DistributedConsumptionResult
        {
            FlatId = f.Id,
            FlatNumber = f.Number,
            ShareCount = f.ShareCount,
            DistributedConsumption = Math.Round((totalShared * f.ShareCount) / totalShare, 2)
        }).ToList();

        return results;
    }
} 