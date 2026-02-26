using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.EventHandlers;

public class ExpenseCreatedEventHandler : INotificationHandler<ExpenseCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebPushService _webPushService;

    public ExpenseCreatedEventHandler(IApplicationDbContext context, IWebPushService webPushService)
    {
        _context = context;
        _webPushService = webPushService;
    }

    public async Task Handle(ExpenseCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Get all managers and admins
        var managers = await _context.Users
            .Where(u => u.IsActive && (u.Role == UserRole.Admin || u.Role == UserRole.Manager))
            .ToListAsync(cancellationToken);

        var title = "Yeni Gider Kaydı";
        var message = $"{notification.Title} için {notification.Amount:C2} tutarında yeni bir gider kaydedildi.";

        foreach (var manager in managers)
        {
            // 2. Persist In-App Notification
            var inAppNotification = new Notification
            {
                UserId = manager.Id,
                Title = title,
                Message = message,
                Type = "ExpenseCreated",
                RelatedEntityId = notification.ExpenseId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(inAppNotification);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // 3. Send Web Push to Managers
        var managerIds = managers.Select(m => m.Id).ToList();
        var subscriptions = await _context.UserPushSubscriptions
            .Where(s => managerIds.Contains(s.UserId))
            .ToListAsync(cancellationToken);

        foreach (var sub in subscriptions)
        {
            await _webPushService.SendNotificationAsync(
                sub.Endpoint, 
                sub.P256dh, 
                sub.Auth, 
                title, 
                message,
                "/expenses"
            );
        }
    }
}
