using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AkyildizYonetim.Application.Notifications.Commands.SendTargetedNotification;

public record SendTargetedNotificationCommand : IRequest<Result>
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = "announcement"; // announcement, debt, private
    public string TargetType { get; init; } = "all";    // all, floor, tenant
    public string? TargetId { get; init; }               // floor number or tenant GUID
    public int? DelayDays { get; init; }
    public bool SendEmail { get; init; } = false;
}

public class SendTargetedNotificationCommandHandler : IRequestHandler<SendTargetedNotificationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebPushService _webPushService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendTargetedNotificationCommandHandler> _logger;

    public SendTargetedNotificationCommandHandler(
        IApplicationDbContext context,
        IWebPushService webPushService,
        INotificationService notificationService,
        ILogger<SendTargetedNotificationCommandHandler> logger)
    {
        _context = context;
        _webPushService = webPushService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendTargetedNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            List<User> targetUsers = new();

            if (request.TargetType == "all")
            {
                targetUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .ToListAsync(cancellationToken);
            }
            else if (request.TargetType == "floor" && !string.IsNullOrEmpty(request.TargetId) && int.TryParse(request.TargetId, out var floorNumber))
            {
                // Find tenants on this floor
                var tenantIds = await _context.Flats
                    .Where(f => f.FloorNumber == floorNumber && !f.IsDeleted && f.TenantId.HasValue)
                    .Select(f => f.TenantId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (tenantIds.Count > 0)
                {
                    targetUsers = await _context.Users
                        .Where(u => u.IsActive && u.TenantId.HasValue && tenantIds.Contains(u.TenantId.Value))
                        .ToListAsync(cancellationToken);
                }
            }
            else if (request.TargetType == "tenant" && !string.IsNullOrEmpty(request.TargetId) && Guid.TryParse(request.TargetId, out var tenantId))
            {
                targetUsers = await _context.Users
                    .Where(u => u.IsActive && u.TenantId == tenantId)
                    .ToListAsync(cancellationToken);
            }

            if (targetUsers.Count == 0)
                return Result.Failure("Hedef kitlede kullanıcı bulunamadı.");

            string finalTitle = request.Title;
            string finalMessage = request.Message;

            // For debt type: optionally enrich message with overdue debt info
            if (request.Type == "debt" && request.TargetType == "tenant"
                && !string.IsNullOrEmpty(request.TargetId) && Guid.TryParse(request.TargetId, out var debtTenantId))
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-(request.DelayDays ?? 0));
                var overdueDebts = await _context.UtilityDebts
                    .AsNoTracking()
                    .Where(d => d.TenantId == debtTenantId && !d.IsDeleted
                                && d.RemainingAmount > 0
                                && d.DueDate < cutoffDate)
                    .ToListAsync(cancellationToken);

                if (overdueDebts.Count > 0)
                {
                    var total = overdueDebts.Sum(d => d.RemainingAmount);
                    finalMessage += $"\n\nToplam gecikmiş borç: {total:C} ({overdueDebts.Count} kalem)";

                    // Optionally send email
                    if (request.SendEmail)
                    {
                        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == debtTenantId, cancellationToken);
                        if (tenant != null && !string.IsNullOrEmpty(tenant.ContactPersonEmail))
                        {
                            await _notificationService.SendOverdueDebtReminderAsync(
                                debtTenantId,
                                overdueDebts,
                                tenant.ContactPersonEmail,
                                tenant.CompanyName ?? tenant.ContactPersonName,
                                cancellationToken);
                        }
                    }
                }
            }

            // Create in-app notifications
            var notifications = targetUsers.Select(u => new Notification
            {
                UserId = u.Id,
                Title = finalTitle,
                Message = finalMessage,
                Type = request.Type == "debt" ? "Debt" : "Announcement",
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);

            // Send web push
            var userIds = targetUsers.Select(u => u.Id).ToHashSet();
            var subscriptions = await _context.UserPushSubscriptions
                .Where(s => userIds.Contains(s.UserId))
                .ToListAsync(cancellationToken);

            foreach (var sub in subscriptions)
            {
                try
                {
                    await _webPushService.SendNotificationAsync(
                        sub.Endpoint, sub.P256dh, sub.Auth,
                        finalTitle, finalMessage, "/notifications");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Web push gönderilemedi: {Endpoint}", sub.Endpoint);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hedefli bildirim gönderilemedi.");
            return Result.Failure($"Bildirim gönderilemedi: {ex.Message}");
        }
    }
}
