using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Tenants.Commands.DeleteTenant;

public record DeleteTenantCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Flats)
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);
            
        if (tenant == null)
            return Result.Failure("Kiracı bulunamadı.");
            
        // Kiracıyı sil
        tenant.IsDeleted = true;
        tenant.UpdatedAt = DateTime.UtcNow;
        
        // Kiracıya ait flat'ları temizle
        foreach (var flat in tenant.Flats)
        {
            flat.TenantId = null;
            flat.IsOccupied = false;
            flat.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
} 