using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Owners.Commands.CreateOwner;

public record CreateOwnerCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApartmentNumber { get; init; } = string.Empty;
    public decimal MonthlyDues { get; init; }
    public List<string> Flats { get; init; } = new();
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
        // Önce aynı e-postaya sahip silinmiş veya aktif bir kayıt olup olmadığını kontrol et
        var existingOwner = await _context.Owners
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Email == request.Email, cancellationToken);

        Guid ownerId;

        if (existingOwner != null)
        {
            if (!existingOwner.IsDeleted)
            {
                return Result<Guid>.Failure("Bu e-posta adresiyle kayıtlı aktif bir mal sahibi zaten mevcut.");
            }

            // Silinmiş kaydı geri yükle (Restore)
            existingOwner.IsDeleted = false;
            existingOwner.IsActive = true;
            existingOwner.FirstName = request.FirstName;
            existingOwner.LastName = request.LastName;
            existingOwner.PhoneNumber = request.PhoneNumber;
            existingOwner.ApartmentNumber = request.ApartmentNumber;
            existingOwner.MonthlyDues = request.MonthlyDues;
            existingOwner.UpdatedAt = DateTime.UtcNow;
            
            ownerId = existingOwner.Id;
        }
        else
        {
            // Yeni kayıt oluştur
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
            ownerId = owner.Id;
        }

        // Katları atama
        if (request.Flats != null && request.Flats.Any())
        {
            // Bu mal sahibine daha önce atanmış katları bul
            var oldFlats = await _context.Flats
                .Where(f => f.OwnerId == ownerId)
                .ToListAsync(cancellationToken);

            foreach (var f in oldFlats)
            {
                f.OwnerId = null;
            }

            // Yeni seçilen katları ata
            var newFlats = await _context.Flats
                .Where(f => request.Flats.Contains(f.Code))
                .ToListAsync(cancellationToken);

            foreach (var f in newFlats)
            {
                f.OwnerId = ownerId;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(ownerId);
    }
}
