using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.ServiceRequests.Commands.DeleteServiceRequest;

public record DeleteServiceRequestCommand(Guid Id) : IRequest<Result>;

public class DeleteServiceRequestCommandHandler : IRequestHandler<DeleteServiceRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteServiceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteServiceRequestCommand request, CancellationToken ct)
    {
        var sr = await _context.ServiceRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (sr == null) return Result.Failure("Talep bulunamadı.");

        _context.ServiceRequests.Remove(sr);
        await _context.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}
