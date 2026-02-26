using AkyildizYonetim.Application.Common.Interfaces;
using BCrypt.Net;

namespace AkyildizYonetim.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool VerifyLegacyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword)) return false;

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        var base64Hash = Convert.ToBase64String(hash);

        return base64Hash == hashedPassword;
    }
}
