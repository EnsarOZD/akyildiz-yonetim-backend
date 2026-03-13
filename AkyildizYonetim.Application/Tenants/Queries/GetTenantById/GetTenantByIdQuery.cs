using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery : IRequest<Result<TenantDto>>
{
	public Guid Id { get; init; }
}

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
	private readonly IApplicationDbContext _context;
	private readonly ICurrentUserService _currentUserService;

	public GetTenantByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService) 
	{ 
		_context = context; 
		_currentUserService = currentUserService;
	}

	public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken ct)
	{
		// Isolate access for non-admin/manager roles
		if (!_currentUserService.IsAdmin && !_currentUserService.IsManager)
		{
			if (_currentUserService.TenantId != request.Id)
			{
				return Result<TenantDto>.Failure("Bu kiracı verisine erişim yetkiniz yok.");
			}
		}

		var tenant = await _context.Tenants
			.AsNoTracking()
			.Where(t => t.Id == request.Id && !t.IsDeleted)
			.Select(t => new TenantDto
			{
				Id = t.Id,
				CompanyName = t.CompanyName,
				BusinessType = t.BusinessType,
				IdentityNumber = t.IdentityNumber,
				ContactPersonName = t.ContactPersonName,
				ContactPersonPhone = t.ContactPersonPhone,
				ContactPersonEmail = t.ContactPersonEmail,
				MonthlyAidat = t.MonthlyAidat,
				IsActive = t.IsActive,
				CreatedAt = t.CreatedAt,
				UpdatedAt = t.UpdatedAt,

				TotalBalance = _context.UtilityDebts
					.Where(d => d.TenantId == t.Id && !d.IsDeleted && d.Status != Domain.Entities.DebtStatus.Paid)
					.Sum(d => d.RemainingAmount) -
							   _context.AdvanceAccounts
					.Where(a => a.TenantId == t.Id && !a.IsDeleted && a.IsActive)
					.Sum(a => a.Balance),

				AdvanceBalance = _context.AdvanceAccounts
					.Where(a => a.TenantId == t.Id && !a.IsDeleted && a.IsActive)
					.Sum(a => a.Balance),

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
