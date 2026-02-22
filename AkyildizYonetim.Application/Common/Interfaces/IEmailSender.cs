namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendInvitationEmailAsync(string toEmail, string invitationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}