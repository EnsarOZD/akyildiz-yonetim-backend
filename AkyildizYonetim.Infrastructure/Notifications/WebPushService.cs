using AkyildizYonetim.Application.Common.Interfaces;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace AkyildizYonetim.Infrastructure.Notifications;

public class WebPushService : IWebPushService
{
    private readonly PushServiceClient? _pushServiceClient;
    private readonly string _subject;
    private readonly string _publicKey;
    private readonly string _privateKey;

    public WebPushService(IConfiguration configuration)
    {
        _publicKey = configuration["WebPush:VapidPublicKey"] ?? string.Empty;
        _privateKey = configuration["WebPush:VapidPrivateKey"] ?? string.Empty;
        _subject = configuration["WebPush:VapidSubject"] ?? "mailto:info@akyildizyunetim.com";

        if (!string.IsNullOrEmpty(_publicKey) && !string.IsNullOrEmpty(_privateKey))
        {
            _pushServiceClient = new PushServiceClient(new HttpClient());
            _pushServiceClient.DefaultAuthentication = new VapidAuthentication(_publicKey, _privateKey)
            {
                Subject = _subject
            };
        }
    }

    public async Task SendNotificationAsync(string endpoint, string p256dh, string auth, string title, string message, string? url = null)
    {
        if (_pushServiceClient == null) return;

        var subscription = new PushSubscription
        {
            Endpoint = endpoint,
            Keys = new Dictionary<string, string>
            {
                { "p256dh", p256dh },
                { "auth", auth }
            }
        };

        var payload = JsonSerializer.Serialize(new
        {
            notification = new
            {
                title = title,
                body = message,
                icon = "/logo-default.svg",
                data = new { url = url }
            }
        });

        var pushMessage = new PushMessage(payload)
        {
            Topic = "AkyildizNotifications",
            Urgency = PushMessageUrgency.Normal
        };

        try
        {
            // Use RequestPushMessageDeliveryAsync as confirmed by assembly docs
            await _pushServiceClient.RequestPushMessageDeliveryAsync(subscription, pushMessage);
        }
        catch (PushServiceClientException ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebPush Error: {ex.StatusCode} - {ex.Message}");
        }
    }
}
