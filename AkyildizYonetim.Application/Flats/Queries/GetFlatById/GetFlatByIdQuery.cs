using AutoMapper;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Application.Flats.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Flats.Queries.GetFlatById;

public record GetFlatByIdQuery : IRequest<Result<FlatDto>> { public Guid Id { get; init; } }

public class GetFlatByIdQueryHandler : IRequestHandler<GetFlatByIdQuery, Result<FlatDto>>
{
	private readonly IApplicationDbContext _ctx;
	private readonly IMapper _mapper;
	private readonly IFlatShareCalculator _share;

	public GetFlatByIdQueryHandler(IApplicationDbContext ctx, IMapper mapper, IFlatShareCalculator share)
	{ _ctx = ctx; _mapper = mapper; _share = share; }

	public async Task<Result<FlatDto>> Handle(GetFlatByIdQuery request, CancellationToken ct)
	{
		var entity = await _ctx.Flats
			.AsNoTracking()
			.Include(f => f.Owner).Include(f => f.Tenant)
			.FirstOrDefaultAsync(f => f.Id == request.Id && !f.IsDeleted, ct);

		if (entity is null) return Result<FlatDto>.Failure("Ünite bulunamadı.");

		// Aynı gruptakileri çek (hesap için hafif bir sorgu)
		var groupKey = entity.GroupKey ?? $"__{entity.Id}";
		var groupMembers = await _ctx.Flats.AsNoTracking()
			.Where(x => (x.GroupKey ?? $"__{x.Id}") == groupKey && !x.IsDeleted)
			.ToListAsync(ct);

		var shares = _share.ComputeEffectiveShares(groupMembers);

		var dto = _mapper.Map<FlatDto>(entity);
		dto.EffectiveShare = shares.TryGetValue(entity.Id, out var s) ? s : null;

		return Result<FlatDto>.Success(dto);
	}
}
