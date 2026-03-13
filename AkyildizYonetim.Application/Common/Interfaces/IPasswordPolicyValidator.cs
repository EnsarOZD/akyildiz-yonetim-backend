namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IPasswordPolicyValidator
{
    (bool IsValid, string? ErrorMessage) Validate(string password);
}
