using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Email;
using Microsoft.Extensions.Configuration;
using PostmarkDotNet;

namespace AkyildizYonetim.Infrastructure.Email;

public class PostmarkEmailSender : IEmailSender
{
    private readonly string _serverToken;
    private readonly string _fromEmail;
    private readonly string _appName;

    public PostmarkEmailSender(IConfiguration configuration)
    {
        _serverToken = configuration["POSTMARK_SERVER_TOKEN"] 
            ?? throw new InvalidOperationException("POSTMARK_SERVER_TOKEN configuration is missing.");

        _fromEmail = configuration["EMAIL_FROM"] 
            ?? "noreply@akyildizyonetim.com";

        _appName = configuration["APP_NAME"] ?? "Akyıldız Yönetim";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendInvitationEmailAsync(string toEmail, string invitationLink)
    {
        await SendAsync(
            toEmail, 
            $"{_appName} - Davet", 
            EmailTemplates.Invitation(_appName, invitationLink));
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        await SendAsync(
            toEmail, 
            $"{_appName} - Şifre Sıfırlama", 
            EmailTemplates.PasswordReset(_appName, resetLink));
    }

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var client = new PostmarkClient(_serverToken);

        var message = new PostmarkMessage
        {
            To = to,
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };

        var response = await client.SendMessageAsync(message);

        if (response.Status != PostmarkStatus.Success)
        {
            throw new Exception($"Postmark e-posta gönderim hatası: {response.Message} (Kod: {response.ErrorCode})");
        }

        System.Console.WriteLine($"✅ Postmark üzerinden e-posta başarıyla gönderildi: {to}");
    }
}
