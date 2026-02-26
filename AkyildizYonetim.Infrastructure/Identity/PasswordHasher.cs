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
            // Eski SHA-256 hash'leri veya hatalı formatlar için geçici fallback veya safe failure
            return false;
        }
    }
}
