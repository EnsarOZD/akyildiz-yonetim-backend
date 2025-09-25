using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkyildizYonetim.Application.MeterReadings.Commands
{
	public class DistributeSharedConsumptionCommandValidator : AbstractValidator<DistributeSharedConsumptionCommand>
	{
		public DistributeSharedConsumptionCommandValidator()
		{
			RuleFor(x => x.PeriodYear).InclusiveBetween(2000, 2100);
			RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12);
			RuleFor(x => x.SharedAreaConsumption).GreaterThanOrEqualTo(0);
			RuleFor(x => x.MescitConsumption).GreaterThanOrEqualTo(0);
		}
	}
}
