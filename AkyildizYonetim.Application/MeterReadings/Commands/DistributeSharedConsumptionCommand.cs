using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands;

public record DistributeSharedConsumptionCommand : IRequest<List<DistributedConsumptionResult>>
{
	public decimal SharedAreaConsumption { get; init; }
	public decimal MescitConsumption { get; init; }
	public int PeriodYear { get; init; }
	public int PeriodMonth { get; init; }
	public MeterType MeterType { get; init; } = MeterType.Electricity; // Varsayılan elektrik

	// Opsiyonel: Ortak tüketim sayılan sayaçların FlatId'leri
	public List<Guid>? SharedMeterFlatIds { get; init; }
}

public class DistributedConsumptionResult
{
	public Guid FlatId { get; set; }
	public string FlatNumber { get; set; } = string.Empty;
	public int ShareCount { get; set; }
	public decimal DistributedConsumption { get; set; }
}

public class DistributeSharedConsumptionCommandHandler
	: IRequestHandler<DistributeSharedConsumptionCommand, List<DistributedConsumptionResult>>
{
	private readonly IApplicationDbContext _context;
	public DistributeSharedConsumptionCommandHandler(IApplicationDbContext context) => _context = context;

	public async Task<List<DistributedConsumptionResult>> Handle(
		DistributeSharedConsumptionCommand request, CancellationToken cancellationToken)
	{
		// 1) Dönemde belirtilen tip okuması olan daireler
		var flatsInPeriod = await _context.MeterReadings
			.Where(mr => !mr.IsDeleted
						 && mr.Type == request.MeterType
						 && mr.PeriodYear == request.PeriodYear
						 && mr.PeriodMonth == request.PeriodMonth)
			.Select(mr => mr.FlatId)
			.Distinct()
			.ToListAsync(cancellationToken);

		// 2) Aktif + döneme dahil daireler, hisse ile birlikte
		var activeFlats = await _context.Flats
			.Where(f => f.IsActive && !f.IsDeleted && flatsInPeriod.Contains(f.Id))
			.Select(f => new
			{
				f.Id,
				f.Code, // ApartmentNumber yerine Code kullan
				ShareCount = f.ShareCount > 0 ? f.ShareCount : 1 // 0'a karşı varsayılan 1
			})
			.ToListAsync(cancellationToken);

		if (activeFlats.Count == 0)
			return new List<DistributedConsumptionResult>();

		// 3) Toplam ortak tüketim: varsa ortak sayaç FlatId'lerinden oku; yoksa manuel toplamı kullan
		decimal totalSharedConsumption;
		if (request.SharedMeterFlatIds != null && request.SharedMeterFlatIds.Count > 0)
		{
			totalSharedConsumption = await _context.MeterReadings
				.Where(mr => !mr.IsDeleted
							 && mr.Type == request.MeterType
							 && mr.PeriodYear == request.PeriodYear
							 && mr.PeriodMonth == request.PeriodMonth
							 && request.SharedMeterFlatIds.Contains(mr.FlatId))
				.SumAsync(mr => mr.Consumption, cancellationToken);

			totalSharedConsumption = Math.Max(0m, totalSharedConsumption);
		}
		else
		{
			totalSharedConsumption = Math.Max(0m, request.SharedAreaConsumption + request.MescitConsumption);
		}

		var totalShares = activeFlats.Sum(x => Math.Max(0, x.ShareCount));
		if (totalShares <= 0 || totalSharedConsumption <= 0)
			return new List<DistributedConsumptionResult>();

		// 4) Birim pay (kWh/m³) = toplam ortak / toplam hisse
		var perShare = Math.Round(totalSharedConsumption / totalShares, 4, MidpointRounding.AwayFromZero);

		// 5) Dağıtım
		var results = activeFlats.Select(f => new DistributedConsumptionResult
		{
			FlatId = f.Id,
			FlatNumber = f.Code, // ApartmentNumber yerine Code kullan
			ShareCount = f.ShareCount,
			DistributedConsumption = Math.Round(perShare * f.ShareCount, 2, MidpointRounding.AwayFromZero)
		}).ToList();

		// 6) Yuvarlama farkını kapatma (opsiyonel)
		var allocated = results.Sum(r => r.DistributedConsumption);
		var diff = Math.Round(totalSharedConsumption - allocated, 2, MidpointRounding.AwayFromZero);
		if (diff != 0)
		{
			var step = diff > 0 ? 0.01m : -0.01m;
			var n = (int)Math.Abs(diff * 100);
			for (int i = 0; i < n && i < results.Count; i++)
				results[i].DistributedConsumption = Math.Round(results[i].DistributedConsumption + step, 2, MidpointRounding.AwayFromZero);
		}

		return results;
	}
}