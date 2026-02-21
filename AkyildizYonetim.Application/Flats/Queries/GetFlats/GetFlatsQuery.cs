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
        private readonly ICurrentUserService _currentUserService;

        public GetFlatsQueryHandler(IApplicationDbContext ctx, IMapper mapper, IFlatShareCalculator share, ICurrentUserService currentUserService)
        {
            _ctx = ctx;
            _mapper = mapper;
            _share = share;
            _currentUserService = currentUserService;
        }

        public async Task<Result<List<FlatSummaryDto>>> Handle(GetFlatsQuery rq, CancellationToken ct)
{
    var q = _ctx.Flats.AsNoTracking()
        .Include(f => f.Owner).Include(f => f.Tenant)
        .Where(f => !f.IsDeleted);

    // Veri İzolasyonu (RBAC)
    if (!_currentUserService.IsAdmin && !_currentUserService.IsManager)
    {
        if (_currentUserService.TenantId.HasValue)
        {
            q = q.Where(f => f.TenantId == _currentUserService.TenantId.Value);
        }
        else if (_currentUserService.OwnerId.HasValue)
        {
            q = q.Where(f => f.OwnerId == _currentUserService.OwnerId.Value);
        }
        else
        {
            return Result<List<FlatSummaryDto>>.Success(new List<FlatSummaryDto>());
        }
    }

    if (rq.OwnerId.HasValue)     q = q.Where(f => f.OwnerId == rq.OwnerId);
    if (rq.TenantId.HasValue)     q = q.Where(f => f.TenantId == rq.TenantId);
    if (!string.IsNullOrWhiteSpace(rq.Code)) q = q.Where(f => f.Code == rq.Code);
    if (rq.FloorNumber.HasValue)  q = q.Where(f => f.FloorNumber == rq.FloorNumber);
    if (rq.Type.HasValue)         q = q.Where(f => f.Type == rq.Type);
    if (rq.IsOccupied.HasValue)   q = q.Where(f => f.IsOccupied == rq.IsOccupied);
    if (rq.IsActive.HasValue)     q = q.Where(f => f.IsActive == rq.IsActive);

    // 1) DTO listesi (Mapping artık GroupKey ve GroupStrategy içeriyor)
    var list = await q.ProjectTo<FlatSummaryDto>(_mapper.ConfigurationProvider).ToListAsync(ct);

    if (list.Count == 0)
        return Result<List<FlatSummaryDto>>.Success(list);

    // 2) Pay hesaplaması için gerekli tüm üyeleri getir:
    // SQL'deki OPENJSON hatasını ('$' yakınındaki sözdizimi yanlış) önlemek için 
    // ve performansı korumak için ilgili tüm Flat'leri tek seferde çekip hafızada filtreliyoruz.
    var groupKeys = list.Where(l => l.GroupKey != null).Select(l => l.GroupKey).Distinct().ToList();
    var listIds = list.Select(l => l.Id).ToHashSet();

    var allFlats = await _ctx.Flats.AsNoTracking()
        .Where(f => !f.IsDeleted)
        .ToListAsync(ct);

    var membersByKey = allFlats
        .Where(f => (f.GroupKey != null && groupKeys.Contains(f.GroupKey)) || listIds.Contains(f.Id))
        .ToList();

    var shares = _share.ComputeEffectiveShares(membersByKey);

    foreach (var dto in list)
        dto.EffectiveShare = shares.TryGetValue(dto.Id, out var s) ? s : 1m;

    // Örnek sıralama (Kat numarasına göre iniş)
    list = list.OrderByDescending(d => d.FloorNumber ?? int.MinValue).ToList();

    return Result<List<FlatSummaryDto>>.Success(list);
}

    }
}
