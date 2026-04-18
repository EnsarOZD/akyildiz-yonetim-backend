using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Payments.Queries.GetPayments;

public record GetPaymentsQuery : IRequest<Result<PagedResult<PaymentDto>>>
{
    public PaymentType? Type { get; init; }
    public PaymentStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DebtType? UtilityType { get; init; }
    public DebtorType? DebtorType { get; init; }
    public bool ExcludeAdvanceUse { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<PagedResult<PaymentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Data Scope Resolution
        var effectiveTenantId = DataScopeHelper.ResolveTenantId(_currentUserService, request.TenantId, u => u.IsAdmin, u => u.IsManager, u => u.IsDataEntry, u => u.IsObserver);
        var effectiveOwnerId = DataScopeHelper.ResolveOwnerId(_currentUserService, request.OwnerId, u => u.IsAdmin, u => u.IsManager, u => u.IsDataEntry, u => u.IsObserver);

        if (DataScopeHelper.IsScopeRestricted(_currentUserService, u => u.IsAdmin, u => u.IsManager, u => u.IsDataEntry, u => u.IsObserver))
        {
            if (!effectiveTenantId.HasValue && !effectiveOwnerId.HasValue)
            {
                return Result<PagedResult<PaymentDto>>.Success(new PagedResult<PaymentDto>());
            }
        }

        if (effectiveTenantId.HasValue)
            query = query.Where(p => p.TenantId == effectiveTenantId.Value);

        if (effectiveOwnerId.HasValue)
            query = query.Where(p => p.OwnerId == effectiveOwnerId.Value);

        if (request.DebtorType.HasValue)
        {
            if (request.DebtorType == Domain.Entities.DebtorType.OnlyTenants)
                query = query.Where(p => p.TenantId != null);
            else if (request.DebtorType == Domain.Entities.DebtorType.OnlyOwners)
                query = query.Where(p => p.TenantId == null);
        }

        if (request.UtilityType.HasValue)
        {
            query = query.Where(p => _context.PaymentDebts
                .Any(pd => pd.PaymentId == p.Id && pd.Debt.Type == request.UtilityType.Value));
        }

        if (request.Type.HasValue)
            query = query.Where(p => p.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);


        if (request.StartDate.HasValue)
            query = query.Where(p => p.PaymentDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(p => p.PaymentDate <= request.EndDate.Value);

        if (request.ExcludeAdvanceUse)
        {
            // Simplified for SQL translation compatibility
            query = query.Where(p => 
                p.ReceiptNumber == null || 
                !p.ReceiptNumber.StartsWith("AVANS-"));
        }

        var pagedPayments = await query
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.CreatedAt) // Deterministic ordering
            .ThenByDescending(p => p.Id) 
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Type = p.Type,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                Description = p.Description,
                ReceiptNumber = p.ReceiptNumber,
                OwnerId = p.OwnerId,
                TenantId = p.TenantId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                TenantName = p.Tenant != null ? (!string.IsNullOrEmpty(p.Tenant.CompanyName) ? p.Tenant.CompanyName : p.Tenant.ContactPersonName) : null,
                OwnerName = p.Owner != null ? $"{p.Owner.FirstName} {p.Owner.LastName}".Trim() : null,
                FlatInfo = p.Tenant != null && p.Tenant.Flats.Any() 
                    ? string.Join(", ", p.Tenant.Flats.Select(f => $"Daire {f.Number}"))
                    : (p.Owner != null && p.Owner.Flats.Any() ? string.Join(", ", p.Owner.Flats.Select(f => $"Daire {f.Number}")) : null),
                PeriodYear = p.PaymentDebts.Select(pd => pd.Debt.PeriodYear).FirstOrDefault(),
                PeriodMonth = p.PaymentDebts.Select(pd => pd.Debt.PeriodMonth).FirstOrDefault(),
                DebtTypes = p.PaymentDebts.Select(pd => pd.Debt.Type).Distinct().ToList()
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result<PagedResult<PaymentDto>>.Success(pagedPayments);
    }
} 