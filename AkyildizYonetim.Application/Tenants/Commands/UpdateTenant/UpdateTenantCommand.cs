using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApartmentNumber { get; init; } = string.Empty;
    public DateTime LeaseStartDate { get; init; }
    public DateTime? LeaseEndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);
        if (tenant == null)
            return Result.Failure("Kiracı bulunamadı.");
        tenant.FirstName = request.FirstName;
        tenant.LastName = request.LastName;
        tenant.PhoneNumber = request.PhoneNumber;
        tenant.Email = request.Email;
        tenant.ApartmentNumber = request.ApartmentNumber;
        tenant.LeaseStartDate = request.LeaseStartDate;
        tenant.LeaseEndDate = request.LeaseEndDate;
        tenant.MonthlyRent = request.MonthlyRent;
        tenant.IsActive = request.IsActive;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 