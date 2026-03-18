using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.Commands.PostAnnouncement;

public record PostAnnouncementCommand : IRequest<Result>
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool SendEmail { get; init; } = false;
}

public class PostAnnouncementCommandHandler : IRequestHandler<PostAnnouncementCommand, Result>
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public PostAnnouncementCommandHandler(
        IMediator mediator,
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _mediator = mediator;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(PostAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            return Result.Failure("Başlık ve mesaj boş olamaz.");

        // Trigger the broadcast in-app notification event
        await _mediator.Publish(new AnnouncementPublishedEvent(request.Title, request.Message), cancellationToken);

        // Optionally send email to all active users
        if (request.SendEmail)
        {
            var users = await _context.Users
                .Where(u => u.IsActive && !string.IsNullOrEmpty(u.Email))
                .ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                var displayName = $"{user.FirstName} {user.LastName}".Trim();
                await _notificationService.SendAnnouncementEmailAsync(
                    request.Title,
                    request.Message,
                    user.Email,
                    displayName,
                    cancellationToken);
            }
        }

        return Result.Success();
    }
}
