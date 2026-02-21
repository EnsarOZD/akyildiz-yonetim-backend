using System;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Role { get; }
    Guid? TenantId { get; }
    Guid? OwnerId { get; }
    bool IsAdmin { get; }
    bool IsManager { get; }
}
