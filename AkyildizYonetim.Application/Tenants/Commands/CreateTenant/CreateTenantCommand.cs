using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<Result<Guid>>
{
    // İş Yeri Bilgileri
    public string CompanyName { get; init; } = string.Empty;
    public string BusinessType { get; init; } = string.Empty;
    public string CompanyType { get; init; } = string.Empty; // Individual, Corporate
    public string IdentityNumber { get; init; } = string.Empty; // TC/Vergi No

    // İletişim
    public string ContactPersonName { get; init; } = string.Empty;
    public string ContactPersonPhone { get; init; } = string.Empty;
    public string ContactPersonEmail { get; init; } = string.Empty;

    // Lokasyon (tekli/çoklu ve kat fallback)
    public Guid? FlatId { get; init; }            // tek seçim (geri uyum)
    public List<Guid>? FlatIds { get; init; }     // ÇOKLU SEÇİM  ✅
    public int? FloorNumber { get; init; }        // fallback: kat seç, ilk uygun üniteyi ata

    // Aidat & Sözleşme
    public decimal MonthlyAidat { get; init; }
    public DateTime? ContractStartDate { get; init; }
    public DateTime? ContractEndDate { get; init; }

    public bool IsActive { get; init; } = true;
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateTenantCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        // 0) Giriş kontrolleri
        var identity = (request.IdentityNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(identity))
            return Result<Guid>.Failure("Kimlik/Vergi numarası zorunludur.");

        if (request.ContractStartDate.HasValue && request.ContractEndDate.HasValue &&
            request.ContractStartDate > request.ContractEndDate)
            return Result<Guid>.Failure("Sözleşme başlangıç tarihi bitiş tarihinden büyük olamaz.");

        // 1) Atanacak ünite ID setini hazırla (FlatIds + FlatId + Floor fallback)
        var ids = new HashSet<Guid>();
        if (request.FlatIds is { Count: > 0 })
            foreach (var id in request.FlatIds)
                if (id != Guid.Empty) ids.Add(id);

        if (request.FlatId.HasValue && request.FlatId.Value != Guid.Empty)
            ids.Add(request.FlatId.Value);

        if (ids.Count == 0 && request.FloorNumber.HasValue)
    {
        var available = await _context.Flats
            .Where(f => f.FloorNumber == request.FloorNumber.Value
                        && !f.IsDeleted
                        && f.IsActive
                        && !f.IsOccupied
                        // && f.Type != UnitType.Parking   // ❌ KALDIRILDI
                        )
            .OrderBy(f => f.Code)
            .FirstOrDefaultAsync(ct);

        if (available == null)
            return Result<Guid>.Failure($"Seçilen katta ({request.FloorNumber.Value}) boş ünite bulunamadı.");

        ids.Add(available.Id);
    }

        if (ids.Count == 0)
            return Result<Guid>.Failure("Kat (FloorNumber) seçin veya en az bir ünite (FlatId/FlatIds) belirtin.");

        // 2) Transaction
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        // 3) Aynı kimlikte aktif kiracı var mı?
        var existing = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.IdentityNumber == identity, ct);

        Tenant tenant;
        if (existing == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName?.Trim() ?? string.Empty,
                BusinessType = request.BusinessType?.Trim() ?? string.Empty,
                CompanyType = request.CompanyType?.Trim() ?? string.Empty,
                IdentityNumber = identity,
                ContactPersonName = request.ContactPersonName?.Trim() ?? string.Empty,
                ContactPersonPhone = request.ContactPersonPhone?.Trim() ?? string.Empty,
                ContactPersonEmail = request.ContactPersonEmail?.Trim() ?? string.Empty,
                MonthlyAidat = request.MonthlyAidat,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                IsActive = request.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(ct);
        }
        else if (existing.IsDeleted)
        {
            // Soft-deleted → geri getir
            existing.IsDeleted = false;
            existing.CompanyName = request.CompanyName?.Trim();
            existing.BusinessType = request.BusinessType?.Trim();
            existing.CompanyType = request.CompanyType?.Trim();
            existing.ContactPersonName = request.ContactPersonName?.Trim();
            existing.ContactPersonPhone = request.ContactPersonPhone?.Trim();
            existing.ContactPersonEmail = request.ContactPersonEmail?.Trim();
            existing.MonthlyAidat = request.MonthlyAidat;
            existing.ContractStartDate = request.ContractStartDate;
            existing.ContractEndDate = request.ContractEndDate;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Tenants.Update(existing);
            await _context.SaveChangesAsync(ct);
            tenant = existing;
        }
        else
        {
            // Varsayılan davranış: aynı kimlikle ikinci “create”a izin verme
            // (Sonradan ünite ekleme için Update/Attach endpoint’i açmak en sağlıklısı)
            return Result<Guid>.Failure("Bu kimlik numarası ile zaten aktif bir kiracı var.");
        }

        // 4) Seçilen üniteleri çek ve doğrula
        var flats = await _context.Flats
        .Where(f => ids.Contains(f.Id) && !f.IsDeleted)
        .ToListAsync(ct);

    if (flats.Count != ids.Count)
        return Result<Guid>.Failure("Seçilen ünitelerden bazıları bulunamadı.");

    // Sadece pasif/doluysa reddet: tip kısıtı YOK ✅
    var notAvailable = flats.Where(f => !f.IsActive || f.IsOccupied)
    .Select(f => f.Code ?? f.Id.ToString())
    .ToList();

    if (notAvailable.Any())
        return Result<Guid>.Failure($"Uygun olmayan ünite(ler): {string.Join(", ", notAvailable)}");

    // 5) Eşleştirme (değişmedi)
    foreach (var f in flats)
    {
        f.TenantId  = tenant.Id;
        f.IsOccupied = true;
        f.UpdatedAt  = DateTime.UtcNow;
        _context.Flats.Update(f);
    }

    await _context.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    return Result<Guid>.Success(tenant.Id);
    }
}
