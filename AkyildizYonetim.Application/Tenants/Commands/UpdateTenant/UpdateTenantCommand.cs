using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    
    // İş Yeri Bilgileri
    public string CompanyName { get; init; } = string.Empty;
    public string BusinessType { get; init; } = string.Empty;
    public string TaxNumber { get; init; } = string.Empty;
    
    // İletişim Kişisi Bilgileri
    public string ContactPersonName { get; init; } = string.Empty;
    public string ContactPersonPhone { get; init; } = string.Empty;
    public string ContactPersonEmail { get; init; } = string.Empty;
    
    // Aidat ve Borç Yönetimi
    public decimal MonthlyAidat { get; init; }
    
    // Sözleşme Bilgileri (Opsiyonel)
    public DateTime? ContractStartDate { get; init; }
    public DateTime? ContractEndDate { get; init; }
    
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
        
        tenant.CompanyName = request.CompanyName;
        tenant.BusinessType = request.BusinessType;
        tenant.TaxNumber = request.TaxNumber;
        tenant.ContactPersonName = request.ContactPersonName;
        tenant.ContactPersonPhone = request.ContactPersonPhone;
        tenant.ContactPersonEmail = request.ContactPersonEmail;
        tenant.MonthlyAidat = request.MonthlyAidat;
        tenant.ContractStartDate = request.ContractStartDate;
        tenant.ContractEndDate = request.ContractEndDate;
        tenant.IsActive = request.IsActive;
        tenant.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 