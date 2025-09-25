using AutoMapper;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.Flats.Commands.UpdateFlat;

public record UpdateFlatCommand : IRequest<Result>
{
	public required UpdateFlatDto Dto { get; init; }
}

public class UpdateFlatCommandHandler : IRequestHandler<UpdateFlatCommand, Result>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;

	public UpdateFlatCommandHandler(IApplicationDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<Result> Handle(UpdateFlatCommand request, CancellationToken ct)
	{
		var dto = request.Dto;

		// 1) Var mı?
		var entity = await _context.Flats
			.FirstOrDefaultAsync(f => f.Id == dto.Id && !f.IsDeleted, ct);

		if (entity is null)
			return Result.Failure("Ünite bulunamadı.");

		// 2) Code tekilliği (kendi dışında)
		var codeExists = await _context.Flats
			.AnyAsync(f => f.Id != dto.Id && !f.IsDeleted && f.Code == dto.Code, ct);
		if (codeExists)
			return Result.Failure("Bu Code ile başka bir ünite zaten mevcut.");

		// 3) DTO -> Entity (AutoMapper)
		_mapper.Map(dto, entity);

		// 4) Tip bazlı temizlik/güvence
		if (entity.Type == UnitType.Parking)
		{
			entity.FloorNumber = null;
			entity.GroupKey = null;
			entity.Section = null;
			entity.GroupStrategy = GroupStrategy.None;
		}
		else if (entity.GroupStrategy != GroupStrategy.SplitIfMultiple)
		{
			// Split olmayanlarda grup alanlarını nötrle
			entity.GroupKey = null;
			entity.Section = null;
		}

		entity.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(ct);
		return Result.Success();
	}
}
