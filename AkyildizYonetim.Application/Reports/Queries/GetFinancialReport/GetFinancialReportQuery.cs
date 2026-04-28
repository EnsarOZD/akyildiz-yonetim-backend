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

            var paymentsList = await paymentsQuery.ToListAsync(cancellationToken);

            // Borçlar (DueDate'e göre filtreleme - Daha doğru finansal raporlama)
            var debtsQuery = _context.UtilityDebts
                .AsNoTracking()
                .FilterBySecurityContext(_currentUserService)
                .Where(d => d.DueDate >= request.StartDate && d.DueDate <= request.EndDate && !d.IsDeleted);

            if (request.TenantId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                debtsQuery = debtsQuery.Where(d => d.TenantId == request.TenantId);
            
            if (request.OwnerId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                debtsQuery = debtsQuery.Where(d => d.OwnerId == request.OwnerId);

            var debtsList = await debtsQuery.ToListAsync(cancellationToken);

            // Avans hesapları
            var advanceAccountsQuery = _context.AdvanceAccounts
                .AsNoTracking()
                .FilterBySecurityContext(_currentUserService);

            if (request.TenantId.HasValue && (_currentUserService.IsAdmin || _currentUserService.IsManager))
                advanceAccountsQuery = advanceAccountsQuery.Where(aa => aa.TenantId == request.TenantId);

            var advanceAccounts = await advanceAccountsQuery.ToListAsync(cancellationToken);

            // Detaylı liste oluştur (Kronolojik)
            var details = new List<FinancialReportDetailDto>();

            foreach (var p in paymentsList)
            {
                details.Add(new FinancialReportDetailDto
                {
                    Id = p.Id,
                    Date = p.PaymentDate,
                    TenantId = p.TenantId,
                    OwnerId = p.OwnerId,
                    PaymentAmount = p.Amount,
                    Description = p.Description ?? "Ödeme Kaydı",
                    Type = "Payment"
                });
            }

            foreach (var d in debtsList)
            {
                details.Add(new FinancialReportDetailDto
                {
                    Id = d.Id,
                    Date = d.DueDate,
                    TenantId = d.TenantId,
                    OwnerId = d.OwnerId,
                    DebtAmount = d.Amount,
                    RemainingAmount = d.RemainingAmount,
                    Description = $"{d.Type} Borcu ({d.PeriodYear}/{d.PeriodMonth})",
                    Type = "Debt"
                });
            }

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
                    TotalAmount = paymentsList.Sum(p => p.Amount),
                    Count = paymentsList.Count,
                    ByType = paymentsList.GroupBy(p => p.Type)
                        .Select(g => new PaymentTypeSummary
                        {
                            Type = g.Key,
                            Amount = g.Sum(p => p.Amount),
                            Count = g.Count()
                        }).ToList()
                },
                Debts = new DebtSummary
                {
                    TotalAmount = debtsList.Sum(d => d.Amount),
                    TotalPaid = debtsList.Sum(d => d.PaidAmount ?? 0),
                    TotalRemaining = debtsList.Sum(d => d.RemainingAmount),
                    Count = debtsList.Count,
                    ByStatus = debtsList.GroupBy(d => d.Status)
                        .Select(g => new DebtStatusSummary
                        {
                            Status = g.Key,
                            Amount = g.Sum(d => d.Amount),
                            Count = g.Count()
                        }).ToList(),
                    ByType = debtsList.GroupBy(d => d.Type)
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
                },
                Details = details.OrderBy(d => d.Date).ToList()
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
    public List<FinancialReportDetailDto> Details { get; set; } = new();
}

public class FinancialReportDetailDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal DebtAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Payment" or "Debt"
    public string? TenantName { get; set; }
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