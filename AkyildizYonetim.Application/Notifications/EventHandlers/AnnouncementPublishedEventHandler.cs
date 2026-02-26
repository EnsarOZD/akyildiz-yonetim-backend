using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.EventHandlers;

public class AnnouncementPublishedEventHandler : INotificationHandler<AnnouncementPublishedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebPushService _webPushService;

    public AnnouncementPublishedEventHandler(IApplicationDbContext context, IWebPushService webPushService)
    {
        _context = context;
        _webPushService = webPushService;
    }

    public async Task Handle(AnnouncementPublishedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Get all active users to send notifications
        var activeUsers = await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var user in activeUsers)
        {
            // 2. Persist In-App Notification
            var inAppNotification = new Notification
            {
                UserId = user.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = "Announcement",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(inAppNotification);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // 3. Send Web Push to all subscribed devices
        var subscriptions = await _context.UserPushSubscriptions.ToListAsync(cancellationToken);

        foreach (var sub in subscriptions)
        {
            await _webPushService.SendNotificationAsync(
                sub.Endpoint, 
                sub.P256dh, 
                sub.Auth, 
                notification.Title, 
                notification.Message,
                "/dashboard"
            );
        }
    }
}
