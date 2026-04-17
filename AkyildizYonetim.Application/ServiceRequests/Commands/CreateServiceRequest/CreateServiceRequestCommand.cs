using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;

namespace AkyildizYonetim.Application.ServiceRequests.Commands.CreateServiceRequest;

public record CreateServiceRequestCommand(
    string Title,
    string Description,
    ServiceRequestCategory Category
) : IRequest<Result<Guid>>;

public class CreateServiceRequestCommandHandler : IRequestHandler<CreateServiceRequestCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateServiceRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateServiceRequestCommand request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<Guid>.Failure("Talep başlığı boş olamaz.");
            if (string.IsNullOrWhiteSpace(request.Description))
                return Result<Guid>.Failure("Talep açıklaması boş olamaz.");

            var sr = new ServiceRequest
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Category = request.Category,
                Status = ServiceRequestStatus.Open,
                TenantId = _currentUser.TenantId,
                OwnerId = _currentUser.OwnerId
            };

            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync(ct);

            return Result<Guid>.Success(sr.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Talep oluşturulamadı: {ex.Message}");
        }
    }
}
