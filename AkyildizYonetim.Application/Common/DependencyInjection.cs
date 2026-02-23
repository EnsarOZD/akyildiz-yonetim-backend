using AkyildizYonetim.Application.Common.Behaviors;
using AkyildizYonetim.Application.Flats.Services;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AkyildizYonetim.Application.Common
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			// MediatR: bu assembly’deki tüm handler’lar
			services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

			// AutoMapper: bu assembly’deki tüm Profile sınıfları (Common/Mapping/* dahil)
			services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);

			// FluentValidation: bu assembly’deki tüm validator’lar
			services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

			// Unit pay hesabı servisi
			services.AddSingleton<IFlatShareCalculator, FlatShareCalculator>();

			return services;
		}
	}
}
