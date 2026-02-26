namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    bool VerifyLegacyPassword(string password, string hashedPassword);
}
