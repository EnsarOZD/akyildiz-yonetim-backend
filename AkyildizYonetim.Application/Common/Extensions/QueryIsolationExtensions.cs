using System.Linq.Expressions;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.Common.Extensions;

public static class QueryIsolationExtensions
{
    public static IQueryable<T> FilterBySecurityContext<T>(
        this IQueryable<T> query, 
        ICurrentUserService currentUserService) where T : class
    {
        // Admin and Manager can see everything in their respective domains
        if (currentUserService.IsAdmin || currentUserService.IsManager)
        {
            return query;
        }

        // Apply Tenant isolation
        if (currentUserService.TenantId.HasValue)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            
            // Check if T has TenantId property
            var tenantIdProperty = typeof(T).GetProperty("TenantId");
            if (tenantIdProperty != null && tenantIdProperty.PropertyType == typeof(Guid?))
            {
                var property = Expression.Property(parameter, "TenantId");
                var value = Expression.Constant(currentUserService.TenantId.Value, typeof(Guid?));
                var equal = Expression.Equal(property, value);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);
                return query.Where(lambda);
            }
            
            // Special case for Tenant entity itself
            if (typeof(T) == typeof(Tenant))
            {
                var idProperty = typeof(T).GetProperty("Id");
                var property = Expression.Property(parameter, "Id");
                var value = Expression.Constant(currentUserService.TenantId.Value);
                var equal = Expression.Equal(property, value);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);
                return query.Where(lambda);
            }
        }

        // Apply Owner isolation
        if (currentUserService.OwnerId.HasValue)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            
            // Check if T has OwnerId property
            var ownerIdProperty = typeof(T).GetProperty("OwnerId");
            if (ownerIdProperty != null && ownerIdProperty.PropertyType == typeof(Guid?))
            {
                var property = Expression.Property(parameter, "OwnerId");
                var value = Expression.Constant(currentUserService.OwnerId.Value, typeof(Guid?));
                var equal = Expression.Equal(property, value);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);
                return query.Where(lambda);
            }

            // Special case for Owner entity itself
            if (typeof(T) == typeof(Owner))
            {
                var idProperty = typeof(T).GetProperty("Id");
                var property = Expression.Property(parameter, "Id");
                var value = Expression.Constant(currentUserService.OwnerId.Value);
                var equal = Expression.Equal(property, value);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);
                return query.Where(lambda);
            }
        }

        // If no context and not admin, return empty to be safe
        return query.Where(e => false);
    }
}
