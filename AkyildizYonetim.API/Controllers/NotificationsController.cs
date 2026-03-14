using AkyildizYonetim.Application.Notifications.Commands.MarkAllAsRead;
using AkyildizYonetim.Application.Notifications.Commands.MarkAsRead;
using AkyildizYonetim.Application.Notifications.Commands.PushSubscription;
using AkyildizYonetim.Application.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace AkyildizYonetim.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public NotificationsController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
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
        // For now, map 'all' targets directly to a global broadcast.
        if (request.TargetType == "all" || request.Type == "announcement")
        {
            var command = new AkyildizYonetim.Application.Notifications.Commands.PostAnnouncement.PostAnnouncementCommand
            {
                Title = request.Title,
                Message = request.Message
            };
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
        }

        // Targeted specifics (floor, tenant, debt) are placeholders for future implementation.
        return BadRequest("Kişiye/Kata özel bildirimler ve borç hatırlatmaları yakında aktif edilecektir. Lütfen şimdilik 'Genel Duyuru' seçeneğini kullanın.");
    }
}

public class TargetedNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "announcement"; // announcement, debt, private
    public string TargetType { get; set; } = "all"; // all, floor, tenant
    public int? TargetId { get; set; }
    public int? DelayDays { get; set; }
}

public class PushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
