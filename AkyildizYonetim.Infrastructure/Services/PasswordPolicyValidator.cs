using AkyildizYonetim.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace AkyildizYonetim.Infrastructure.Services;

public class PasswordPolicyValidator : IPasswordPolicyValidator
{
    public (bool IsValid, string? ErrorMessage) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return (false, "Şifre en az 8 karakter uzunluğunda olmalıdır.");
        }

        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            return (false, "Şifre en az bir büyük harf içermelidir.");
        }

        if (!Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]"))
        {
            return (false, "Şifre en az bir özel karakter içermelidir.");
        }

        return (true, null);
    }
}
