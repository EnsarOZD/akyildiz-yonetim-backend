using System.Threading.Tasks;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IWebPushService
{
    Task SendNotificationAsync(string endpoint, string p256dh, string auth, string title, string message, string? url = null);
}
