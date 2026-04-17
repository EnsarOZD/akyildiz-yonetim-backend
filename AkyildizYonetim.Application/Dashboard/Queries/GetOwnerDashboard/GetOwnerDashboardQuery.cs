using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Dashboard.Queries.GetOwnerDashboard;

public record GetOwnerDashboardQuery : IRequest<Result<OwnerDashboardDto>>;

public class GetOwnerDashboardQueryHandler
    : IRequestHandler<GetOwnerDashboardQuery, Result<OwnerDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOwnerDashboardQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<OwnerDashboardDto>> Handle(GetOwnerDashboardQuery request, CancellationToken ct)
    {
        try
        {
            var ownerId = _currentUser.OwnerId;
            if (!ownerId.HasValue)
                return Result<OwnerDashboardDto>.Failure("Owner kimliği bulunamadı.");

            var debts = await _context.UtilityDebts
                .AsNoTracking()
                .Where(d => d.OwnerId == ownerId.Value && d.Status != DebtStatus.Paid)
                .Join(
                    _context.Flats.AsNoTracking(),
                    d => d.FlatId,
                    f => f.Id,
                    (d, f) => new OwnerDebtDto
                    {
                        Id = d.Id,
                        Type = d.Type.ToString(),
                        PeriodYear = d.PeriodYear,
                        PeriodMonth = d.PeriodMonth,
                        Amount = d.Amount,
                        PaidAmount = d.PaidAmount ?? 0,
                        RemainingAmount = d.RemainingAmount,
                        DueDate = d.DueDate,
                        Status = d.Status.ToString(),
                        IsOverdue = d.DueDate < DateTime.UtcNow,
                        FlatCode = !string.IsNullOrEmpty(f.Code) ? f.Code : f.Number
                    })
                .ToListAsync(ct);

            var tenants = await _context.Flats
                .AsNoTracking()
                .Where(f => f.OwnerId == ownerId.Value && f.TenantId != null)
                .Join(
                    _context.Tenants.AsNoTracking(),
                    f => f.TenantId,
                    t => t.Id,
                    (f, t) => new OwnerTenantDto
                    {
                        TenantId = t.Id,
                        DisplayName = !string.IsNullOrEmpty(t.CompanyName) ? t.CompanyName : t.ContactPersonName,
                        FlatCode = !string.IsNullOrEmpty(f.Code) ? f.Code : f.Number,
                        IsActive = t.IsActive
                    })
                .ToListAsync(ct);

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.OwnerId == ownerId.Value)
                .OrderByDescending(p => p.PaymentDate)
                .Take(10)
                .Select(p => new OwnerPaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Description = p.Description,
                    Type = p.Type.ToString()
                })
                .ToListAsync(ct);

            var dto = new OwnerDashboardDto
            {
                MyDebts = debts,
                MyTenants = tenants,
                RecentPayments = payments,
                TotalOwnerDebt = debts.Sum(d => d.RemainingAmount),
                OverdueCount = debts.Count(d => d.IsOverdue)
            };

            return Result<OwnerDashboardDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OwnerDashboardDto>.Failure($"Panel verisi alınamadı: {ex.Message}");
        }
    }
}

public class OwnerDashboardDto
{
    public List<OwnerDebtDto> MyDebts { get; set; } = new();
    public List<OwnerTenantDto> MyTenants { get; set; } = new();
    public List<OwnerPaymentDto> RecentPayments { get; set; } = new();
    public decimal TotalOwnerDebt { get; set; }
    public int OverdueCount { get; set; }
}

public class OwnerDebtDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public string? FlatCode { get; set; }
}

public class OwnerTenantDto
{
    public Guid TenantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FlatCode { get; set; }
    public bool IsActive { get; set; }
}

public class OwnerPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
}
