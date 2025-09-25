using AkyildizYonetim.Domain.Entities;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.Flats.Services;

public interface IFlatShareCalculator
{
	Dictionary<Guid, decimal> ComputeEffectiveShares(IEnumerable<Flat> units);
}

public class FlatShareCalculator : IFlatShareCalculator
{
	public Dictionary<Guid, decimal> ComputeEffectiveShares(IEnumerable<Flat> units)
	{
		var result = new Dictionary<Guid, decimal>();
		var grouped = units.GroupBy(u => string.IsNullOrWhiteSpace(u.GroupKey) ? $"__{u.Id}" : u.GroupKey);

		foreach (var g in grouped)
		{
			var members = g.ToList();
			var split = members.Any(m => m.GroupStrategy == GroupStrategy.SplitIfMultiple);
			var occupied = members.Where(m => m.IsOccupied).ToList();

			if (split)
			{
				var n = occupied.Count;
				foreach (var m in members)
					result[m.Id] = m.IsOccupied ? (n == 0 ? 0m : 1m / n) : 0m;
			}
			else
			{
				foreach (var m in members)
					result[m.Id] = m.IsOccupied ? 1m : 0m;
			}
		}
		return result;
	}
}
