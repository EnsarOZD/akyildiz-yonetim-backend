using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.AdvanceAccounts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccounts;

public record GetAdvanceAccountsQuery : IRequest<Result<List<AdvanceAccountDto>>>
{
    public Guid? TenantId { get; init; }
    public bool? ActiveOnly { get; init; }
}

public class GetAdvanceAccountsQueryHandler : IRequestHandler<GetAdvanceAccountsQuery, Result<List<AdvanceAccountDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAdvanceAccountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AdvanceAccountDto>>> Handle(GetAdvanceAccountsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.AdvanceAccounts
                .Include(aa => aa.Tenant)
                .AsQueryable();

            if (request.TenantId.HasValue)
            {
                query = query.Where(aa => aa.TenantId == request.TenantId.Value);
            }

            if (request.ActiveOnly == true)
            {
                query = query.Where(aa => aa.IsActive && aa.Balance > 0);
            }

            var advanceAccounts = await query
                .OrderByDescending(aa => aa.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = advanceAccounts.Select(aa => new AdvanceAccountDto
            {
                Id = aa.Id,
                TenantId = aa.TenantId,
                TenantName = aa.Tenant?.CompanyName ?? "Bilinmiyor",
                Balance = aa.Balance,
                Description = aa.Description,
                IsActive = aa.IsActive,
                CreatedAt = aa.CreatedAt,
                UpdatedAt = aa.UpdatedAt ?? aa.CreatedAt
            }).ToList();

            return Result<List<AdvanceAccountDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<AdvanceAccountDto>>.Failure($"Avans hesapları getirilirken hata oluştu: {ex.Message}");
        }
    }
} 