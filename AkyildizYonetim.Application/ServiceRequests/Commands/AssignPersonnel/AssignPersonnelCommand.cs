using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.ServiceRequests.Commands.AssignPersonnel;

public record AssignPersonnelCommand(Guid Id, Guid PersonnelId) : IRequest<Result>;

public class AssignPersonnelCommandHandler : IRequestHandler<AssignPersonnelCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignPersonnelCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignPersonnelCommand request, CancellationToken ct)
    {
        var sr = await _context.ServiceRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (sr == null) return Result.Failure("Talep bulunamadı.");

        sr.AssignedPersonnelId = request.PersonnelId;
        sr.Status = ServiceRequestStatus.InProgress;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
