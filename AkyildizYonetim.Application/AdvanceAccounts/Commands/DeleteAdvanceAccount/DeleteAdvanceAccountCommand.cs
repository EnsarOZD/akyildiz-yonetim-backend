using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.DeleteAdvanceAccount;

public record DeleteAdvanceAccountCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteAdvanceAccountCommandHandler : IRequestHandler<DeleteAdvanceAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdvanceAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteAdvanceAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var advanceAccount = await _context.AdvanceAccounts
                .FirstOrDefaultAsync(aa => aa.Id == request.Id && !aa.IsDeleted, cancellationToken);

            if (advanceAccount == null)
            {
                return Result.Failure("Avans hesabı bulunamadı.");
            }

            // Soft delete
            advanceAccount.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Avans hesabı silinirken hata oluştu: {ex.Message}");
        }
    }
} 