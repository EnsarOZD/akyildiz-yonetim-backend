using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<Result<Guid>>
{
    // İş Yeri Bilgileri
    public string CompanyName { get; init; } = string.Empty;
    public string BusinessType { get; init; } = string.Empty;
    public string TaxNumber { get; init; } = string.Empty;
    
    // İletişim Kişisi Bilgileri
    public string ContactPersonName { get; init; } = string.Empty;
    public string ContactPersonPhone { get; init; } = string.Empty;
    public string ContactPersonEmail { get; init; } = string.Empty;
    
    // Lokasyon Bilgileri (Flat seçimi)
    public Guid FlatId { get; init; } // Hangi üniteyi kiralayacak
    
    // Aidat ve Borç Yönetimi
    public decimal MonthlyAidat { get; init; }
    
    // Sözleşme Bilgileri (Opsiyonel)
    public DateTime? ContractStartDate { get; init; }
    public DateTime? ContractEndDate { get; init; }
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
        // Flat'ın mevcut olup olmadığını ve boş olup olmadığını kontrol et
        var flat = await _context.Flats.FirstOrDefaultAsync(f => f.Id == request.FlatId && !f.IsDeleted, cancellationToken);
        if (flat == null)
            return Result<Guid>.Failure("Seçilen ünite bulunamadı.");
        
        if (flat.IsOccupied)
            return Result<Guid>.Failure("Seçilen ünite zaten dolu.");
        
        // TaxNumber benzersizlik kontrolü
        var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TaxNumber == request.TaxNumber && !t.IsDeleted, cancellationToken);
        if (existingTenant != null)
            return Result<Guid>.Failure("Bu vergi numarası zaten kayıtlı.");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            BusinessType = request.BusinessType,
            TaxNumber = request.TaxNumber,
            ContactPersonName = request.ContactPersonName,
            ContactPersonPhone = request.ContactPersonPhone,
            ContactPersonEmail = request.ContactPersonEmail,
            MonthlyAidat = request.MonthlyAidat,
            ContractStartDate = request.ContractStartDate,
            ContractEndDate = request.ContractEndDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Tenants.Add(tenant);
        
        // Flat'ı kiracıya ata ve dolu olarak işaretle
        flat.TenantId = tenant.Id;
        flat.IsOccupied = true;
        flat.BusinessType = request.BusinessType;
        flat.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(tenant.Id);
    }
} 