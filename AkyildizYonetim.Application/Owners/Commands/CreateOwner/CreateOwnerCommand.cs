using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.Owners.Commands.CreateOwner;

public record CreateOwnerCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApartmentNumber { get; init; } = string.Empty;
    public decimal MonthlyDues { get; init; }
}

public class CreateOwnerCommandHandler : IRequestHandler<CreateOwnerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateOwnerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = new Owner
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            ApartmentNumber = request.ApartmentNumber,
            MonthlyDues = request.MonthlyDues,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Owners.Add(owner);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(owner.Id);
    }
} 