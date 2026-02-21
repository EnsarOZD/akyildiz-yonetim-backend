using System.Net;
using System.Net.Mail;
using AkyildizYonetim.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AkyildizYonetim.Infrastructure.Email;

public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public EmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            System.Console.WriteLine($"📧 E-posta gönderiliyor: {to} | Konu: {subject}");
            
            var message = new MailMessage(_settings.From, to, subject, body)
            {
                IsBodyHtml = true
            };

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.User, _settings.Password),
                EnableSsl = true,
                Timeout = 10000 // 10 saniye timeout
            };

            // Sertifika hataları nedeniyle gönderimin durmaması için (Debug amaçlı)
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            await client.SendMailAsync(message);
            System.Console.WriteLine($"✅ E-posta başarıyla iletildi: {to}");
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"❌ E-posta Gönderim Hatası (SMTP): {ex.Message}");
            if (ex.InnerException != null) 
                System.Console.WriteLine($"🔗 İç Hata: {ex.InnerException.Message}");
        }
    }
}