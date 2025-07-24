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
}

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<List<PaymentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .Where(p => !p.IsDeleted)
            .AsQueryable();

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
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<PaymentDto>>.Success(payments);
    }
} 