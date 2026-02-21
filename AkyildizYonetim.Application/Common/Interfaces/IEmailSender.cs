namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendInvitationEmailAsync(string toEmail, string invitationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}