using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UpdateAdvanceAccount;

public record UpdateAdvanceAccountCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public decimal Balance { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateAdvanceAccountCommandHandler : IRequestHandler<UpdateAdvanceAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdvanceAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAdvanceAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var advanceAccount = await _context.AdvanceAccounts
                .FirstOrDefaultAsync(aa => aa.Id == request.Id && !aa.IsDeleted, cancellationToken);

            if (advanceAccount == null)
            {
                return Result.Failure("Avans hesabı bulunamadı.");
            }

            // Tenant'ın var olup olmadığını kontrol et
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken);

            if (tenant == null)
            {
                return Result.Failure("Kiracı bulunamadı.");
            }

            advanceAccount.TenantId = request.TenantId;
            advanceAccount.Balance = request.Balance;
            advanceAccount.Description = request.Description;
            advanceAccount.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Avans hesabı güncellenirken hata oluştu: {ex.Message}");
        }
    }
} 