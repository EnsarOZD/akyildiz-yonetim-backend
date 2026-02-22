using AkyildizYonetim.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AkyildizYonetim.Infrastructure.Services;

public class AppUrlBuilder : IAppUrlBuilder
{
    private readonly string _baseUrl;

    public AppUrlBuilder(IConfiguration configuration)
    {
        _baseUrl = configuration["APP_BASE_URL"] 
            ?? throw new InvalidOperationException("APP_BASE_URL configuration is missing.");
        
        // Ensure no trailing slash
        _baseUrl = _baseUrl.TrimEnd('/');
    }

    public string BuildInvitationLink(string token, string email)
        => $"{_baseUrl}/#/invite?token={token}&email={email}";

    public string BuildResetPasswordLink(string token, string email)
        => $"{_baseUrl}/#/reset-password?token={token}&email={email}";
}
