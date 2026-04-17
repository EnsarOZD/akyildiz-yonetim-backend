using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.ServiceRequests.Commands.UpdateServiceRequestStatus;

public record UpdateServiceRequestStatusCommand(
    Guid Id,
    ServiceRequestStatus Status,
    string? AdminNote
) : IRequest<Result>;

public class UpdateServiceRequestStatusCommandHandler : IRequestHandler<UpdateServiceRequestStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateServiceRequestStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateServiceRequestStatusCommand request, CancellationToken ct)
    {
        try
        {
            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsDataEntry)
                return Result.Failure("Bu işlem için yetkiniz bulunmamaktadır.");

            var sr = await _context.ServiceRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (sr == null)
                return Result.Failure("Talep bulunamadı.");

            sr.Status = request.Status;
            if (!string.IsNullOrWhiteSpace(request.AdminNote))
                sr.AdminNote = request.AdminNote.Trim();
            if (request.Status == ServiceRequestStatus.Closed)
                sr.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Durum güncellenemedi: {ex.Message}");
        }
    }
}
