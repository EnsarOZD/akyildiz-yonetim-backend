using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.ServiceRequests.Commands.ResolveRequest;

public record ResolveServiceRequestCommand(Guid Id, string ResolutionNote) : IRequest<Result>;

public class ResolveServiceRequestCommandHandler : IRequestHandler<ResolveServiceRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ResolveServiceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ResolveServiceRequestCommand request, CancellationToken ct)
    {
        var sr = await _context.ServiceRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (sr == null) return Result.Failure("Talep bulunamadı.");

        sr.ResolutionNote = request.ResolutionNote;
        sr.Status = ServiceRequestStatus.Resolved;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
