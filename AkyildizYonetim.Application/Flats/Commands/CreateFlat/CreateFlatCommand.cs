using AutoMapper;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.Flats.Commands.CreateFlat;

public record CreateFlatCommand : IRequest<Result<Guid>>
{
	public required CreateFlatDto Dto { get; init; }
}

public class CreateFlatCommandHandler : IRequestHandler<CreateFlatCommand, Result<Guid>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;

	public CreateFlatCommandHandler(IApplicationDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<Result<Guid>> Handle(CreateFlatCommand request, CancellationToken ct)
	{
		var dto = request.Dto;

		// 1) Ýþ kurallarý/guard: Code tekil mi? (IsDeleted=false kayýtlar içinde)
		var exists = await _context.Flats
			.AnyAsync(f => !f.IsDeleted && f.Code == dto.Code, ct);
		if (exists)
			return Result<Guid>.Failure("Bu Code ile kayýt zaten mevcut.");

		// 2) DTO -> Entity
		var entity = _mapper.Map<Flat>(dto);
		entity.Id = Guid.NewGuid();
		entity.CreatedAt = DateTime.UtcNow;

		// 3) Ek güvenlik: Parking ise FloorNumber/grup alanlarýný temizle
		if (entity.Type == UnitType.Parking)
		{
			entity.FloorNumber = null;
			entity.GroupKey = null;
			entity.Section = null;
			entity.GroupStrategy = GroupStrategy.None;
		}

		_context.Flats.Add(entity);
		await _context.SaveChangesAsync(ct);

		return Result<Guid>.Success(entity.Id);
	}
}
