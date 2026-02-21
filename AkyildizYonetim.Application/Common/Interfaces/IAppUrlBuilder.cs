namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IAppUrlBuilder
{
    string BuildInvitationLink(string token, string email);
    string BuildResetPasswordLink(string token, string email);
}
