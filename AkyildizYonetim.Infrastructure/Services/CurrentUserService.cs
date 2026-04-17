using System;
using System.Security.Claims;
using AkyildizYonetim.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AkyildizYonetim.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public Guid? TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
            return Guid.TryParse(tenantId, out var result) ? result : null;
        }
    }

    public Guid? OwnerId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("OwnerId");
            return Guid.TryParse(tenantId, out var result) ? result : null;
        }
    }

    public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
    
    public bool IsManager => Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) ?? false;

    public bool IsDataEntry => Role?.Equals("DataEntry", StringComparison.OrdinalIgnoreCase) ?? false;

    public bool IsObserver => Role?.Equals("Observer", StringComparison.OrdinalIgnoreCase) ?? false;

    public bool IsOwner => Role?.Equals("Owner", StringComparison.OrdinalIgnoreCase) ?? false;
}
