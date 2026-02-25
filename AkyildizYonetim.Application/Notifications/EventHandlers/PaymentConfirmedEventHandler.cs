using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.EventHandlers;

public class PaymentConfirmedEventHandler : INotificationHandler<PaymentConfirmedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebPushService _webPushService;

    public PaymentConfirmedEventHandler(IApplicationDbContext context, IWebPushService webPushService)
    {
        _context = context;
        _webPushService = webPushService;
    }

    public async Task Handle(PaymentConfirmedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Persist In-App Notification
        var inAppNotification = new Notification
        {
            UserId = notification.UserId,
            Title = "Ödeme Alındı",
            Message = $"{notification.Amount:C2} tutarındaki ödemeniz onaylanmıştır. Teşekkür ederiz.",
            Type = "Payment",
            RelatedEntityId = notification.PaymentId,
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
