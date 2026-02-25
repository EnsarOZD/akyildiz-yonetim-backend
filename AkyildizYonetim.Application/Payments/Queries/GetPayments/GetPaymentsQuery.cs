using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.DTOs;

namespace AkyildizYonetim.Application.Payments.Queries.GetPayments;

public record GetPaymentsQuery : IRequest<Result<List<PaymentDto>>>
{
    public PaymentType? Type { get; init; }
    public PaymentStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DebtType? UtilityType { get; init; }
}

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<List<PaymentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Veri İzolasyonu (RBAC)
        if (!_currentUserService.IsAdmin && !_currentUserService.IsManager)
        {
            if (_currentUserService.TenantId.HasValue)
            {
                query = query.Where(p => p.TenantId == _currentUserService.TenantId.Value);
            }
            else if (_currentUserService.OwnerId.HasValue)
            {
                query = query.Where(p => p.OwnerId == _currentUserService.OwnerId.Value);
            }
            else
            {
                // Eğer rolü var ama bağlı olduğu bir ID yoksa (ve admin değilse), veri görmemeli
                return Result<List<PaymentDto>>.Success(new List<PaymentDto>());
            }
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

        if (request.OwnerId.HasValue)
            query = query.Where(p => p.OwnerId == request.OwnerId.Value);

        if (request.TenantId.HasValue)
            query = query.Where(p => p.TenantId == request.TenantId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(p => p.PaymentDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(p => p.PaymentDate <= request.EndDate.Value);

        var payments = await query
            .Include(p => p.Tenant)
            .Include(p => p.Owner)
            .Include(p => p.PaymentDebts)
                .ThenInclude(pd => pd.Debt)
            .OrderByDescending(p => p.PaymentDate)
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
                PeriodMonth = p.PaymentDebts.Select(pd => pd.Debt.PeriodMonth).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return Result<List<PaymentDto>>.Success(payments);
    }
} 