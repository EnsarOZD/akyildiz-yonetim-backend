using System;
using System.Linq;
using AkyildizYonetim.Application.Common.Interfaces;

namespace AkyildizYonetim.Application.Common.Extensions;

/// <summary>
/// Provides explicit scope resolution for tenant/owner-scoped queries.
/// Each caller declares which roles see all data for their specific use case.
/// </summary>
public static class DataScopeHelper
{
    /// <summary>
    /// Resolves the effective TenantId given the current user and the request-supplied value.
    /// If the user's role is in fullAccessRoles, the request value is trusted.
    /// Otherwise, the user's own claim overrides the request value.
    /// </summary>
    public static Guid? ResolveTenantId(
        ICurrentUserService user, 
        Guid? requestTenantId, 
        params Func<ICurrentUserService, bool>[] fullAccessChecks)
    {
        if (fullAccessChecks.Any(check => check(user)))
            return requestTenantId;

        return user.TenantId;
    }

    /// <summary>
    /// Resolves the effective OwnerId given the current user and the request-supplied value.
    /// If the user's role is in fullAccessRoles, the request value is trusted.
    /// Otherwise, the user's own claim overrides the request value.
    /// </summary>
    public static Guid? ResolveOwnerId(
        ICurrentUserService user, 
        Guid? requestOwnerId, 
        params Func<ICurrentUserService, bool>[] fullAccessChecks)
    {
        if (fullAccessChecks.Any(check => check(user)))
            return requestOwnerId;

        return user.OwnerId;
    }

    /// <summary>
    /// Returns true if the current user has no tenant/owner claim
    /// AND is not in a full-access role -> should see restricted results or fail.
    /// </summary>
    public static bool IsScopeRestricted(
        ICurrentUserService user, 
        params Func<ICurrentUserService, bool>[] fullAccessChecks)
    {
        if (fullAccessChecks.Any(check => check(user)))
            return false;

        return !user.TenantId.HasValue && !user.OwnerId.HasValue;
    }
}
