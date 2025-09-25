using AutoMapper;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Application.DTOs;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.Common.Mapping;

public class FlatsProfile : Profile
{
	public FlatsProfile()
	{
		// Entity -> DTO (detay)
		CreateMap<Flat, FlatDto>()
			// EffectiveShare DB'de yok; handler içinde set edeceğiz
			.ForMember(d => d.EffectiveShare, o => o.Ignore());

		// Entity -> DTO (liste/özet)
		CreateMap<Flat, FlatSummaryDto>()
				 .ForMember(d => d.OwnerName, o => o.MapFrom(s =>
					 s.Owner != null
						 ? (((s.Owner.FirstName ?? "") + " " + (s.Owner.LastName ?? "")).Trim())
						 : string.Empty))
				 .ForMember(d => d.TenantCompanyName, o => o.MapFrom(s =>
					 s.Tenant != null ? (s.Tenant.CompanyName ?? null) : null))
				 .ForMember(d => d.EffectiveShare, o => o.Ignore());

		// DTO -> Entity (Create/Update)
		CreateMap<CreateFlatDto, Flat>();
		CreateMap<UpdateFlatDto, Flat>();
	}
}
