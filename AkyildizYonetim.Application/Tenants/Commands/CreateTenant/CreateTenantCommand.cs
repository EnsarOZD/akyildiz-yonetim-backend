using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApartmentNumber { get; init; } = string.Empty;
    public DateTime LeaseStartDate { get; init; }
    public DateTime? LeaseEndDate { get; init; }
    public decimal MonthlyRent { get; init; }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            ApartmentNumber = request.ApartmentNumber,
            LeaseStartDate = request.LeaseStartDate,
            LeaseEndDate = request.LeaseEndDate,
            MonthlyRent = request.MonthlyRent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(tenant.Id);
    }
} 