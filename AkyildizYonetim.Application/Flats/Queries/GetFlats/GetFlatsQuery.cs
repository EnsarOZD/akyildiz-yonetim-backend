using AutoMapper;
using AutoMapper.QueryableExtensions;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Application.Flats.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Domain.Entities.Enums;

namespace AkyildizYonetim.Application.Flats.Queries.GetFlats
{
    public record GetFlatsQuery : IRequest<Result<List<FlatSummaryDto>>>
    {
        public Guid? OwnerId { get; init; }
        public Guid? TenantId { get; init; }
        public string? Code { get; init; }
        public int? FloorNumber { get; init; }
        public FlatEnums.UnitType? Type { get; init; }
        public bool? IsOccupied { get; init; }
        public bool? IsActive { get; init; }
    }

    public class GetFlatsQueryHandler : IRequestHandler<GetFlatsQuery, Result<List<FlatSummaryDto>>>
    {
        private readonly IApplicationDbContext _ctx;
        private readonly IMapper _mapper;
        private readonly IFlatShareCalculator _share;

        public GetFlatsQueryHandler(IApplicationDbContext ctx, IMapper mapper, IFlatShareCalculator share)
        {
            _ctx = ctx;
            _mapper = mapper;
            _share = share;
        }

        public async Task<Result<List<FlatSummaryDto>>> Handle(GetFlatsQuery rq, CancellationToken ct)
{
    var q = _ctx.Flats.AsNoTracking()
        .Include(f => f.Owner).Include(f => f.Tenant)
        .Where(f => !f.IsDeleted);

    if (rq.OwnerId.HasValue)     q = q.Where(f => f.OwnerId == rq.OwnerId);
    if (rq.TenantId.HasValue)     q = q.Where(f => f.TenantId == rq.TenantId);
    if (!string.IsNullOrWhiteSpace(rq.Code)) q = q.Where(f => f.Code == rq.Code);
    if (rq.FloorNumber.HasValue)  q = q.Where(f => f.FloorNumber == rq.FloorNumber);
    if (rq.Type.HasValue)         q = q.Where(f => f.Type == rq.Type);
    if (rq.IsOccupied.HasValue)   q = q.Where(f => f.IsOccupied == rq.IsOccupied);
    if (rq.IsActive.HasValue)     q = q.Where(f => f.IsActive == rq.IsActive);

    // 1) DTO listesi
    var list = await q.ProjectTo<FlatSummaryDto>(_mapper.ConfigurationProvider).ToListAsync(ct);

    if (list.Count == 0)
        return Result<List<FlatSummaryDto>>.Success(list);

    // 2) Pay hesaplaması için gerekli minimal bilgiler
    var ids = list.Select(d => d.Id).ToArray();

    var minimal = await _ctx.Flats.AsNoTracking()
        .Where(f => ids.Contains(f.Id))
        .Select(f => new
        {
            f.Id,
            f.GroupKey,
            f.GroupStrategy
        })
        .ToListAsync(ct);

    var keysWithGroup = minimal
        .Where(x => x.GroupKey != null)
        .Select(x => x.GroupKey!)     // NULL değil
        .Distinct()
        .ToList();

    var idsWithoutGroup = minimal
        .Where(x => x.GroupKey == null)
        .Select(x => x.Id)
        .ToList();

    // 3) Grup üyelerini getir:
    //    - GroupKey'i olanlar: aynı GroupKey'deki tüm üyeler
    //    - GroupKey'i olmayanlar: sadece kendisi
    var membersByKey = await _ctx.Flats.AsNoTracking()
        .Where(f => !f.IsDeleted &&
                   (
                       (f.GroupKey != null && keysWithGroup.Contains(f.GroupKey)) ||
                       idsWithoutGroup.Contains(f.Id)
                   ))
        .ToListAsync(ct);

    var shares = _share.ComputeEffectiveShares(membersByKey);

    foreach (var dto in list)
        dto.EffectiveShare = shares.TryGetValue(dto.Id, out var s) ? s : 1m; // fallback 1

    // örnek sıralama
    list = list.OrderByDescending(d => d.FloorNumber ?? int.MinValue).ToList();

    return Result<List<FlatSummaryDto>>.Success(list);
}

    }
}
