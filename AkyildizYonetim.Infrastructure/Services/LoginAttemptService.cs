using AkyildizYonetim.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AkyildizYonetim.Infrastructure.Services;

public class LoginAttemptService : ILoginAttemptService
{
    private readonly IMemoryCache _cache;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 5;
    private const string CacheKeyPrefix = "LoginAttempt_";

    public LoginAttemptService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> IsLockedOutAsync(string email)
    {
        if (_cache.TryGetValue(GetCacheKey(email, "LockedOut"), out _))
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task RegisterFailedAttemptAsync(string email)
    {
        var attemptsKey = GetCacheKey(email, "Attempts");
        var attempts = _cache.Get<int>(attemptsKey);
        attempts++;

        if (attempts >= MaxFailedAttempts)
        {
            _cache.Set(GetCacheKey(email, "LockedOut"), true, TimeSpan.FromMinutes(LockoutMinutes));
            _cache.Remove(attemptsKey);
        }
        else
        {
            _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(30)); // Reset count after 30 mins of inactivity
        }

        return Task.CompletedTask;
    }

    public Task ResetAttemptsAsync(string email)
    {
        _cache.Remove(GetCacheKey(email, "Attempts"));
        _cache.Remove(GetCacheKey(email, "LockedOut"));
        return Task.CompletedTask;
    }

    public Task<TimeSpan?> GetLockoutRemainingAsync(string email)
    {
        // For IMemoryCache, we don't easily get TTL, but for this PR's requirements, 
        // a simple boolean check is often enough. We'll return null for this simple impl.
        return Task.FromResult<TimeSpan?>(null);
    }

    private string GetCacheKey(string email, string suffix) => $"{CacheKeyPrefix}{email.ToLowerInvariant()}_{suffix}";
}
