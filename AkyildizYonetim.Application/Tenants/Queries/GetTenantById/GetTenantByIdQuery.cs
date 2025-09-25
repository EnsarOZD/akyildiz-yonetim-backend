using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery : IRequest<Result<TenantDto>>
{
	public Guid Id { get; init; }
}

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
	private readonly IApplicationDbContext _context;
	public GetTenantByIdQueryHandler(IApplicationDbContext context) { _context = context; }

	public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken ct)
	{
		var tenant = await _context.Tenants
			.AsNoTracking()
			.Where(t => t.Id == request.Id && !t.IsDeleted)
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

				// Kiracıya bağlı üniteler (yeni alanlar)
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
			.FirstOrDefaultAsync(ct);

		if (tenant == null)
			return Result<TenantDto>.Failure("Kiracı bulunamadı.");

		return Result<TenantDto>.Success(tenant);
	}
}
