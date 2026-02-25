using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.EventHandlers;

public class DebtCreatedEventHandler : INotificationHandler<DebtCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebPushService _webPushService;

    public DebtCreatedEventHandler(IApplicationDbContext context, IWebPushService webPushService)
    {
        _context = context;
        _webPushService = webPushService;
    }

    public async Task Handle(DebtCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Persist In-App Notification for the Tenant
        var inAppNotification = new Notification
        {
            UserId = notification.UserId,
            Title = "Yeni Borç Tahakkuku",
            Message = $"{notification.Type} için {notification.Amount:C2} tutarında yeni borç eklendi.",
            Type = "Debt",
            RelatedEntityId = notification.DebtId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(inAppNotification);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Send Web Push
        var subscriptions = await _context.UserPushSubscriptions
            .Where(s => s.UserId == notification.UserId)
            .ToListAsync(cancellationToken);

        foreach (var sub in subscriptions)
        {
            await _webPushService.SendNotificationAsync(
                sub.Endpoint, 
                sub.P256dh, 
                sub.Auth, 
                inAppNotification.Title, 
                inAppNotification.Message,
                "/dashboard"
            );
        }
    }
}
