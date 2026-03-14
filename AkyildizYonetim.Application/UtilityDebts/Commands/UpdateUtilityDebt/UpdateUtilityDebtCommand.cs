using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.UpdateUtilityDebt;

public record UpdateUtilityDebtCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public DebtStatus Status { get; init; }
    public decimal? PaidAmount { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? Description { get; init; }
    public string? InvoiceNumber { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public DateTime DueDate { get; init; }
}

public class UpdateUtilityDebtCommandHandler : IRequestHandler<UpdateUtilityDebtCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUtilityDebtCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService) 
    { 
        _context = context; 
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateUtilityDebtCommand request, CancellationToken cancellationToken)
    {
        var debt = await _context.UtilityDebts.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (debt == null)
            return Result.Failure("Borç kaydı bulunamadı.");

        // Isolate modification for non-admin/manager roles
        if (!_currentUserService.IsAdmin && !_currentUserService.IsManager)
        {
            if (debt.TenantId != _currentUserService.TenantId && debt.OwnerId != _currentUserService.OwnerId)
            {
                return Result.Failure("Bu borç kaydını değiştirme yetkiniz yok.");
            }
        }
            
        debt.Amount = request.Amount;
        debt.Status = request.Status;
        debt.PaidAmount = request.PaidAmount;
        debt.PaidDate = request.PaidDate;
        debt.Description = request.Description;
        debt.InvoiceNumber = request.InvoiceNumber;
        debt.TenantId = request.TenantId;
        debt.OwnerId = request.OwnerId;
        debt.PeriodYear = request.PeriodYear;
        debt.PeriodMonth = request.PeriodMonth;
        debt.DueDate = request.DueDate;
        
        // Kalan tutarı hesapla
        debt.RemainingAmount = debt.Amount - (debt.PaidAmount ?? 0);
        
        debt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 