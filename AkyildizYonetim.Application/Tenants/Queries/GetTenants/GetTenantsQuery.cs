using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Tenants.Queries.GetTenants;

public record GetTenantsQuery : IRequest<Result<List<TenantDto>>>
{
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? Period { get; init; }
    public bool? ShowOnlyOccupied { get; init; }
    public int? FloorNumber { get; init; }
}

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, Result<List<TenantDto>>>
{
	private readonly IApplicationDbContext _context;
	public GetTenantsQueryHandler(IApplicationDbContext context) => _context = context;

	public async Task<Result<List<TenantDto>>> Handle(GetTenantsQuery request, CancellationToken ct)
	{
		var q = _context.Tenants
			.AsNoTracking()
			.Where(t => !t.IsDeleted)
			.AsQueryable();

		if (request.IsActive.HasValue)
			q = q.Where(t => t.IsActive == request.IsActive.Value);

		if (request.Period.HasValue)
		{
			var p = request.Period.Value;
			q = q.Where(t => t.ContractStartDate <= p &&
							(t.ContractEndDate == null || t.ContractEndDate >= p));
		}

		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			var like = $"%{request.SearchTerm.Trim()}%";
			q = q.Where(t =>
				EF.Functions.Like(t.CompanyName, like) ||
				EF.Functions.Like(t.ContactPersonName, like) ||
				EF.Functions.Like(t.IdentityNumber, like) ||
				EF.Functions.Like(t.ContactPersonEmail, like) ||
				EF.Functions.Like(t.ContactPersonPhone, like));
		}

		if (request.ShowOnlyOccupied.HasValue)
		{
			if (request.ShowOnlyOccupied.Value)
			{
				// En az bir dolu ünitesi olan kiracılar
				q = q.Where(t => t.Flats.Any(f => !f.IsDeleted && f.IsOccupied));
			}
			else
			{
				// Hiç dolu ünitesi olmayan kiracılar
				q = q.Where(t => !t.Flats.Any(f => !f.IsDeleted && f.IsOccupied));
			}
		}

		if (request.FloorNumber.HasValue)
		{
			var floor = request.FloorNumber.Value;
			q = q.Where(t => t.Flats.Any(f => !f.IsDeleted && f.FloorNumber == floor));
		}

		var tenants = await q
			.Select(t => new TenantDto
			{
				Id = t.Id,
				CompanyName = t.CompanyName,
				BusinessType = t.BusinessType,
				CompanyType = t.CompanyType,
				IdentityNumber = t.IdentityNumber,
				ContactPersonName = t.ContactPersonName,
				ContactPersonPhone = t.ContactPersonPhone,
				ContactPersonEmail = t.ContactPersonEmail,
				MonthlyAidat = t.MonthlyAidat,
				ContractStartDate = t.ContractStartDate,
				ContractEndDate = t.ContractEndDate,
				IsActive = t.IsActive,
				CreatedAt = t.CreatedAt,
				UpdatedAt = t.UpdatedAt,

				Flats = _context.Flats
					.Where(f => f.TenantId == t.Id && !f.IsDeleted)
					.OrderByDescending(f => f.FloorNumber ?? int.MinValue)
					.Select(f => new TenantFlatInfoDto
					{
						Id = f.Id,
						Code = f.Code,
						FloorNumber = f.FloorNumber,
						Type = f.Type,
						UnitArea = f.UnitArea,
						IsOccupied = f.IsOccupied
					})
					.ToList()
			})
			.ToListAsync(ct);

		return Result<List<TenantDto>>.Success(tenants);
	}
}

public record GetAvailableFlatsQuery : IRequest<Result<List<TenantFlatInfoDto>>>
{
	public int? FloorNumber { get; init; }
	public string? SearchTerm { get; init; }
}

public class GetAvailableFlatsQueryHandler : IRequestHandler<GetAvailableFlatsQuery, Result<List<TenantFlatInfoDto>>>
{
	private readonly IApplicationDbContext _context;
	public GetAvailableFlatsQueryHandler(IApplicationDbContext context) => _context = context;

	public async Task<Result<List<TenantFlatInfoDto>>> Handle(GetAvailableFlatsQuery request, CancellationToken ct)
	{
		var q = _context.Flats
			.AsNoTracking()
			.Where(f => !f.IsDeleted && f.IsActive && !f.IsOccupied)
			.AsQueryable();

		if (request.FloorNumber.HasValue)
			q = q.Where(f => f.FloorNumber == request.FloorNumber.Value);

		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			var like = $"%{request.SearchTerm.Trim()}%";
			q = q.Where(f =>
				EF.Functions.Like(f.Code, like) ||
				EF.Functions.Like(f.Number, like) ||        // legacy alanlar, varsa
				EF.Functions.Like(f.UnitNumber, like));
		}

		var list = await q
			.OrderBy(f => f.FloorNumber ?? int.MinValue)
			.ThenBy(f => f.Code)
			.Select(f => new TenantFlatInfoDto
			{
				Id = f.Id,
				Code = f.Code,
				FloorNumber = f.FloorNumber,
				Type = f.Type,
				UnitArea = f.UnitArea,
				IsOccupied = f.IsOccupied
			})
			.ToListAsync(ct);

		return Result<List<TenantFlatInfoDto>>.Success(list);
	}
}
