using AkyildizYonetim.Application.Notifications.Commands.MarkAllAsRead;
using AkyildizYonetim.Application.Notifications.Commands.MarkAsRead;
using AkyildizYonetim.Application.Notifications.Commands.PushSubscription;
using AkyildizYonetim.Application.Notifications.Commands.SendTargetedNotification;
using AkyildizYonetim.Application.Notifications.Queries.GetNotifications;
using AkyildizYonetim.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AkyildizYonetim.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotificationsController(IMediator mediator, IConfiguration configuration, IApplicationDbContext context, INotificationService notificationService)
    {
        _mediator = mediator;
        _configuration = configuration;
        _context = context;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int take = 20, [FromQuery] int skip = 0)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = unreadOnly,
            Take = take,
            Skip = skip
        });

        return Ok(result);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new MarkAsReadCommand { Id = id, UserId = userId });

        return result.IsSuccess ? Ok() : (result.ErrorMessage == "Bildirim bulunamadı." ? NotFound() : BadRequest(result.ErrorMessage));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new MarkAllAsReadCommand { UserId = userId });

        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }

    [HttpGet("vapid-public-key")]
    public IActionResult GetVapidPublicKey()
    {
        var publicKey = _configuration["WebPush:VapidPublicKey"];
        if (string.IsNullOrEmpty(publicKey))
            return NotFound("VAPID Public Key not configured.");

        return Ok(new { PublicKey = publicKey });
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new SubscribePushCommand
        {
            UserId = userId,
            Endpoint = request.Endpoint,
            P256dh = request.P256dh,
            Auth = request.Auth
        });

        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] string endpoint)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new UnsubscribePushCommand { Endpoint = endpoint, UserId = userId });

        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAllNotifications(CancellationToken cancellationToken)
    {
        var all = await _context.Notifications.ToListAsync(cancellationToken);
        _context.Notifications.RemoveRange(all);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "admin,manager")]
    [HttpPost("broadcast")]
    public async Task<IActionResult> PostAnnouncement([FromBody] AkyildizYonetim.Application.Notifications.Commands.PostAnnouncement.PostAnnouncementCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [Authorize(Roles = "admin,manager")]
    [HttpPost("targeted")]
    public async Task<IActionResult> SendTargetedNotification([FromBody] TargetedNotificationRequest request)
    {
        var result = await _mediator.Send(new SendTargetedNotificationCommand
        {
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            DelayDays = request.DelayDays,
            SendEmail = request.SendEmail
        });
        return result.IsSuccess ? Ok(new { message = "Bildirim başarıyla gönderildi." }) : BadRequest(result.ErrorMessage);
    }

    [Authorize(Roles = "admin,manager")]
    [HttpPost("send-overdue-email")]
    public async Task<IActionResult> SendOverdueEmail([FromBody] SendOverdueEmailRequest request)
    {
        if (!request.TenantId.HasValue && !request.OwnerId.HasValue)
            return BadRequest("TenantId veya OwnerId gereklidir.");

        var cutoffDate = DateTime.UtcNow;

        if (request.TenantId.HasValue)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == request.TenantId.Value && !t.IsDeleted);
            if (tenant == null) return NotFound("Kiracı bulunamadı.");
            if (string.IsNullOrEmpty(tenant.ContactPersonEmail)) return BadRequest("Kiracının e-posta adresi yok.");

            var debts = await _context.UtilityDebts
                .Where(d => d.TenantId == request.TenantId.Value && !d.IsDeleted && d.RemainingAmount > 0 && d.DueDate < cutoffDate)
                .ToListAsync();

            if (debts.Count == 0) return BadRequest("Gecikmiş borç bulunamadı.");

            await _notificationService.SendOverdueDebtReminderAsync(
                tenant.Id, debts, tenant.ContactPersonEmail,
                tenant.CompanyName ?? tenant.ContactPersonName);

            return Ok(new { message = $"{tenant.CompanyName} için {debts.Count} borç hatırlatması e-posta ile gönderildi." });
        }
        else
        {
            var owner = await _context.Owners
                .FirstOrDefaultAsync(o => o.Id == request.OwnerId && !o.IsDeleted);
            if (owner == null) return NotFound("Mal sahibi bulunamadı.");
            if (string.IsNullOrEmpty(owner.Email)) return BadRequest("Mal sahibinin e-posta adresi yok.");

            var debts = await _context.UtilityDebts
                .Where(d => d.OwnerId == request.OwnerId.Value && !d.IsDeleted && d.RemainingAmount > 0 && d.DueDate < cutoffDate)
                .ToListAsync();

            if (debts.Count == 0) return BadRequest("Gecikmiş borç bulunamadı.");

            await _notificationService.SendOverdueDebtReminderAsync(
                owner.Id, debts, owner.Email,
                $"{owner.FirstName} {owner.LastName}");

            return Ok(new { message = $"{owner.FirstName} {owner.LastName} için {debts.Count} borç hatırlatması e-posta ile gönderildi." });
        }
    }
}

public class TargetedNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "announcement"; // announcement, debt, private
    public string TargetType { get; set; } = "all"; // all, floor, tenant
    public string? TargetId { get; set; }             // floor number string or tenant GUID string
    public int? DelayDays { get; set; }
    public bool SendEmail { get; set; } = false;
}

public class SendOverdueEmailRequest
{
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }
}

public class PushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
