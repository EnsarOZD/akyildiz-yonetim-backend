using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

using AkyildizYonetim.Application.Common.Extensions;

namespace AkyildizYonetim.Application.Reports.Queries.GetFinancialReport;

public record GetFinancialReportQuery : IRequest<Result<FinancialReportDto>>
{
    public DateTime StartDate { get; init; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; init; } = DateTime.UtcNow;
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }
}

public class GetFinancialReportQueryHandler 
    : IRequestHandler<GetFinancialReportQuery, Result<FinancialReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetFinancialReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FinancialReportDto>> Handle(
        GetFinancialReportQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Ödemeler
            var paymentsQuery = _context.Payments
                .AsNoTracking()
                .FilterBySecurityContext(_currentUserService)
                .Where(p => p.PaymentDate >= request.StartDate && p.PaymentDate <= request.EndDate && !p.IsDeleted);

            if (request.TenantId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                paymentsQuery = paymentsQuery.Where(p => p.TenantId == request.TenantId);
            
            if (request.OwnerId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                paymentsQuery = paymentsQuery.Where(p => p.OwnerId == request.OwnerId);

            var payments = await paymentsQuery.ToListAsync(cancellationToken);

            // Borçlar
            var debtsQuery = _context.UtilityDebts
                .AsNoTracking()
                .FilterBySecurityContext(_currentUserService)
                .Where(d => d.CreatedAt >= request.StartDate && d.CreatedAt <= request.EndDate && !d.IsDeleted);

            if (request.TenantId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                debtsQuery = debtsQuery.Where(d => d.TenantId == request.TenantId);
            
            if (request.OwnerId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                debtsQuery = debtsQuery.Where(d => d.OwnerId == request.OwnerId);

            var debts = await debtsQuery.ToListAsync(cancellationToken);

            // Avans hesapları
            var advanceAccountsQuery = _context.AdvanceAccounts
                .AsNoTracking()
                .FilterBySecurityContext(_currentUserService)
                .Where(aa => aa.CreatedAt >= request.StartDate && aa.CreatedAt <= request.EndDate && !aa.IsDeleted);

            if (request.TenantId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                advanceAccountsQuery = advanceAccountsQuery.Where(aa => aa.TenantId == request.TenantId);

            var advanceAccounts = await advanceAccountsQuery.ToListAsync(cancellationToken);

            // Rapor verilerini hesapla
            var report = new FinancialReportDto
            {
                Period = new PeriodInfo
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                },
                Payments = new PaymentSummary
                {
                    TotalAmount = payments.Sum(p => p.Amount),
                    Count = payments.Count,
                    ByType = payments.GroupBy(p => p.Type)
                        .Select(g => new PaymentTypeSummary
                        {
                            Type = g.Key,
                            Amount = g.Sum(p => p.Amount),
                            Count = g.Count()
                        }).ToList()
                },
                Debts = new DebtSummary
                {
                    TotalAmount = debts.Sum(d => d.Amount),
                    TotalPaid = debts.Sum(d => d.PaidAmount ?? 0),
                    TotalRemaining = debts.Sum(d => d.RemainingAmount),
                    Count = debts.Count,
                    ByStatus = debts.GroupBy(d => d.Status)
                        .Select(g => new DebtStatusSummary
                        {
                            Status = g.Key,
                            Amount = g.Sum(d => d.Amount),
                            Count = g.Count()
                        }).ToList(),
                    ByType = debts.GroupBy(d => d.Type)
                        .Select(g => new DebtTypeSummary
                        {
                            Type = g.Key,
                            Amount = g.Sum(d => d.Amount),
                            PaidAmount = g.Sum(d => d.PaidAmount ?? 0),
                            RemainingAmount = g.Sum(d => d.RemainingAmount),
                            Count = g.Count()
                        }).ToList()
                },
                AdvanceAccounts = new AdvanceAccountSummary
                {
                    TotalBalance = advanceAccounts.Sum(aa => aa.Balance),
                    Count = advanceAccounts.Count,
                    AverageBalance = advanceAccounts.Any() ? advanceAccounts.Average(aa => aa.Balance) : 0
                }
            };

            return Result<FinancialReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<FinancialReportDto>.Failure($"Rapor oluşturulamadı: {ex.Message}");
        }
    }
}

public class FinancialReportDto
{
    public PeriodInfo Period { get; set; } = null!;
    public PaymentSummary Payments { get; set; } = null!;
    public DebtSummary Debts { get; set; } = null!;
    public AdvanceAccountSummary AdvanceAccounts { get; set; } = null!;
}

public class PeriodInfo
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class PaymentSummary
{
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
    public List<PaymentTypeSummary> ByType { get; set; } = new();
}

public class PaymentTypeSummary
{
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class DebtSummary
{
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }
    public int Count { get; set; }
    public List<DebtStatusSummary> ByStatus { get; set; } = new();
    public List<DebtTypeSummary> ByType { get; set; } = new();
}

public class DebtStatusSummary
{
    public DebtStatus Status { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class DebtTypeSummary
{
    public DebtType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int Count { get; set; }
}

public class AdvanceAccountSummary
{
    public decimal TotalBalance { get; set; }
    public int Count { get; set; }
    public decimal AverageBalance { get; set; }
} 