using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebtById;

public record GetUtilityDebtByIdQuery : IRequest<Result<UtilityDebtDto>>
{
    public Guid Id { get; init; }
}

public class GetUtilityDebtByIdQueryHandler : IRequestHandler<GetUtilityDebtByIdQuery, Result<UtilityDebtDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUtilityDebtByIdQueryHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<UtilityDebtDto>> Handle(GetUtilityDebtByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _context.UtilityDebts.Where(d => d.Id == request.Id && !d.IsDeleted)
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
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (d == null)
            return Result<UtilityDebtDto>.Failure("Borç kaydı bulunamadı.");
        return Result<UtilityDebtDto>.Success(d);
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
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 