using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
} 