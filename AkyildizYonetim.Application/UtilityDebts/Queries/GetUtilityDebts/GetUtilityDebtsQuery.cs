using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebts;

public record GetUtilityDebtsQuery : IRequest<Result<List<UtilityDebtDto>>>
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
}

public class GetUtilityDebtsQueryHandler : IRequestHandler<GetUtilityDebtsQuery, Result<List<UtilityDebtDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUtilityDebtsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<UtilityDebtDto>>> Handle(GetUtilityDebtsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UtilityDebts
            .Include(d => d.Tenant)
            .Include(d => d.Flat)
            .Include(d => d.Owner)
            .IgnoreQueryFilters()
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        // Veri İzolasyonu (RBAC)
        if (!_currentUserService.IsAdmin && !_currentUserService.IsManager && 
            !_currentUserService.IsDataEntry && !_currentUserService.IsObserver)
        {
            if (_currentUserService.TenantId.HasValue)
            {
                query = query.Where(d => d.TenantId == _currentUserService.TenantId.Value);
            }
            else if (_currentUserService.OwnerId.HasValue)
            {
                query = query.Where(d => d.OwnerId == _currentUserService.OwnerId.Value);
            }
            else
            {
                return Result<List<UtilityDebtDto>>.Success(new List<UtilityDebtDto>());
            }
        }

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
        if (request.TenantId.HasValue)
            query = query.Where(d => d.TenantId == request.TenantId.Value);
        if (request.OwnerId.HasValue)
            query = query.Where(d => d.OwnerId == request.OwnerId.Value);
        var debts = await query.OrderByDescending(d => d.PeriodYear).ThenByDescending(d => d.PeriodMonth)
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
            .ToListAsync(cancellationToken);
        return Result<List<UtilityDebtDto>>.Success(debts);
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