using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.AdvanceAccounts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccountById;

public record GetAdvanceAccountByIdQuery : IRequest<Result<AdvanceAccountDto>>
{
    public Guid Id { get; init; }
}

public class GetAdvanceAccountByIdQueryHandler : IRequestHandler<GetAdvanceAccountByIdQuery, Result<AdvanceAccountDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAdvanceAccountByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AdvanceAccountDto>> Handle(GetAdvanceAccountByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var advanceAccount = await _context.AdvanceAccounts
                .Include(aa => aa.Tenant)
                .FirstOrDefaultAsync(aa => aa.Id == request.Id && !aa.IsDeleted, cancellationToken);

            if (advanceAccount == null)
            {
                return Result<AdvanceAccountDto>.Failure("Avans hesabı bulunamadı.");
            }

            var dto = new AdvanceAccountDto
            {
                Id = advanceAccount.Id,
                TenantId = advanceAccount.TenantId,
                TenantName = advanceAccount.Tenant?.CompanyName ?? "Bilinmiyor",
                Balance = advanceAccount.Balance,
                Description = advanceAccount.Description,
                IsActive = advanceAccount.IsActive,
                CreatedAt = advanceAccount.CreatedAt,
                UpdatedAt = advanceAccount.UpdatedAt ?? advanceAccount.CreatedAt
            };

            return Result<AdvanceAccountDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AdvanceAccountDto>.Failure($"Avans hesabı getirilirken hata oluştu: {ex.Message}");
        }
    }
} 