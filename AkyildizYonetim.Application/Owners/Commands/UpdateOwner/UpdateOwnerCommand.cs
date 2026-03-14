using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Owners.Commands.UpdateOwner;

public record UpdateOwnerCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApartmentNumber { get; init; } = string.Empty;
    public decimal MonthlyDues { get; init; }
    public bool IsActive { get; init; }
    public List<string> Flats { get; init; } = new();
}

public class UpdateOwnerCommandHandler : IRequestHandler<UpdateOwnerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateOwnerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateOwnerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var owner = await _context.Owners
                .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken);

            if (owner == null)
            {
                return Result.Failure("Ev sahibi bulunamadı.");
            }

            owner.FirstName = request.FirstName;
            owner.LastName = request.LastName;
            owner.PhoneNumber = request.PhoneNumber;
            owner.Email = request.Email;
            owner.ApartmentNumber = request.ApartmentNumber;
            // Only update MonthlyDues if explicitly sent (non-zero), otherwise preserve existing
            if (request.MonthlyDues > 0)
                owner.MonthlyDues = request.MonthlyDues;
            owner.IsActive = request.IsActive;
            owner.UpdatedAt = DateTime.UtcNow;

            // Katları güncelle
            if (request.Flats != null && request.Flats.Count > 0)
            {
                // Mevcut atamaları temizle
                var currentFlats = await _context.Flats
                    .Where(f => f.OwnerId == owner.Id && !f.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var f in currentFlats)
                {
                    f.OwnerId = null;
                }

                // Yeni seçilen katları ata
                var newFlats = await _context.Flats
                    .Where(f => request.Flats.Contains(f.Code) && !f.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var f in newFlats)
                {
                    f.OwnerId = owner.Id;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] UpdateOwnerCommand: {ex}");
            return Result.Failure($"Mal sahibi güncellenemedi: {ex.Message} {ex.InnerException?.Message}");
        }
    }
}