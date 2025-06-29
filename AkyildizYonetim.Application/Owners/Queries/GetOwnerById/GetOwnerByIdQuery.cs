using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Owners.Queries.GetOwnerById;

public record GetOwnerByIdQuery : IRequest<Result<OwnerDto>>
{
    public Guid Id { get; init; }
}

public class GetOwnerByIdQueryHandler : IRequestHandler<GetOwnerByIdQuery, Result<OwnerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOwnerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OwnerDto>> Handle(GetOwnerByIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await _context.Owners
            .Where(o => o.Id == request.Id && !o.IsDeleted)
            .Select(o => new OwnerDto
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
                PhoneNumber = o.PhoneNumber,
                Email = o.Email,
                ApartmentNumber = o.ApartmentNumber,
                MonthlyDues = o.MonthlyDues,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (owner == null)
        {
            return Result<OwnerDto>.Failure("Ev sahibi bulunamadı.");
        }

        return Result<OwnerDto>.Success(owner);
    }
}

public class OwnerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public decimal MonthlyDues { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 