using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.CreateAdvanceAccount;

public record CreateAdvanceAccountCommand : IRequest<Result<Guid>>
{
    public Guid TenantId { get; init; }
    public decimal Balance { get; init; }
    public string? Description { get; init; }
}

public class CreateAdvanceAccountCommandHandler : IRequestHandler<CreateAdvanceAccountCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateAdvanceAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateAdvanceAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Tenant'ın var olup olmadığını kontrol et
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken);

            if (tenant == null)
            {
                return Result<Guid>.Failure("Kiracı bulunamadı.");
            }

            // Aynı tenant için zaten avans hesabı var mı kontrol et
            var existingAccount = await _context.AdvanceAccounts
                .FirstOrDefaultAsync(aa => aa.TenantId == request.TenantId && !aa.IsDeleted, cancellationToken);

            if (existingAccount != null)
            {
                return Result<Guid>.Failure("Bu kiracı için zaten bir avans hesabı mevcut.");
            }

            var advanceAccount = new Domain.Entities.AdvanceAccount
            {
                TenantId = request.TenantId,
                Balance = request.Balance,
                Description = request.Description,
                IsActive = true
            };

            _context.AdvanceAccounts.Add(advanceAccount);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(advanceAccount.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Avans hesabı oluşturulurken hata oluştu: {ex.Message}");
        }
    }
} 