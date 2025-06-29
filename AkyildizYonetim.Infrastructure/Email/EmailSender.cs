using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AkyildizYonetim.Application.Common.Interfaces;

namespace AkyildizYonetim.Infrastructure.Email;

public class EmailSender : IEmailSender
{
    private readonly string _smtpHost = "mail.akyildizlojistik.com"; // SMTP sunucu adresi
    private readonly int _smtpPort = 587; // SMTP portu
    private readonly string _smtpUser = "info@akyildizlojistik.com"; // SMTP kullanıcı adı
    private readonly string _smtpPass = "VHen98A3"; // SMTP şifresi
    private readonly string _from = "info@akyildizlojistik.com";

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MailMessage(_from, to, subject, body)
        {
            IsBodyHtml = true
        };
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPass),
            EnableSsl = true
        };
        await client.SendMailAsync(message);
    }
} 