using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebts;

public record GetUtilityDebtsQuery : IRequest<Result<PagedResult<UtilityDebtDto>>>
{
    public Guid? FlatId { get; init; }
    public DebtType? Type { get; init; }
    public int? PeriodYear { get; init; }
    public int? PeriodMonth { get; init; }
    public DebtStatus? Status { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetUtilityDebtsQueryHandler : IRequestHandler<GetUtilityDebtsQuery, Result<PagedResult<UtilityDebtDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUtilityDebtsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<UtilityDebtDto>>> Handle(GetUtilityDebtsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UtilityDebts
            .AsNoTracking()
            .AsQueryable();

        // Data Scope Resolution
        var fullAccessRoles = new Func<ICurrentUserService, bool>[] { 
            u => u.IsAdmin, u => u.IsManager, u => u.IsDataEntry, u => u.IsObserver 
        };

        var effectiveTenantId = DataScopeHelper.ResolveTenantId(_currentUserService, request.TenantId, fullAccessRoles);
        var effectiveOwnerId = DataScopeHelper.ResolveOwnerId(_currentUserService, request.OwnerId, fullAccessRoles);

        if (DataScopeHelper.IsScopeRestricted(_currentUserService, fullAccessRoles))
        {
            if (!effectiveTenantId.HasValue && !effectiveOwnerId.HasValue)
            {
                return Result<PagedResult<UtilityDebtDto>>.Success(new PagedResult<UtilityDebtDto>());
            }
        }

        if (effectiveTenantId.HasValue)
            query = query.Where(d => d.TenantId == effectiveTenantId.Value);

        if (effectiveOwnerId.HasValue)
            query = query.Where(d => d.OwnerId == effectiveOwnerId.Value);

        if (request.StartDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
        }

        if (request.FlatId.HasValue)
            query = query.Where(d => d.FlatId == request.FlatId.Value);
        if (request.Type.HasValue)
            query = query.Where(d => d.Type == request.Type.Value);
        if (request.PeriodYear.HasValue)
            query = query.Where(d => d.PeriodYear == request.PeriodYear.Value);
        if (request.PeriodMonth.HasValue)
            query = query.Where(d => d.PeriodMonth == request.PeriodMonth.Value);
        if (request.Status.HasValue)
            query = query.Where(d => d.Status == request.Status.Value);
        var debts = await query.OrderByDescending(d => d.PeriodYear)
            .ThenByDescending(d => d.PeriodMonth)
            .ThenByDescending(d => d.CreatedAt)
            .ThenByDescending(d => d.Id) 
            .Select(d => new UtilityDebtDto
            {
                Id = d.Id,
                FlatId = d.FlatId,
                Type = d.Type,
                PeriodYear = d.PeriodYear,
                PeriodMonth = d.PeriodMonth,
                Amount = d.Amount,
                Status = d.Status,
                PaidAmount = d.PaidAmount,
                PaidDate = d.PaidDate,
                Description = d.Description,
                TenantId = d.TenantId,
                OwnerId = d.OwnerId,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                RemainingAmount = d.RemainingAmount,
                DueDate = d.DueDate,
                TenantName = d.Tenant != null ? d.Tenant.CompanyName : (d.Owner != null ? d.Owner.FirstName + " " + d.Owner.LastName : d.Description),
                FlatInfo = d.Flat != null ? "Daire " + d.Flat.Number : null
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result<PagedResult<UtilityDebtDto>>.Success(debts);
    }
}

public class UtilityDebtDto
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public DebtType Type { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal Amount { get; set; }
    public DebtStatus Status { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public string? TenantName { get; set; }
    public string? FlatInfo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 