namespace AkyildizYonetim.Application.Common.Interfaces;

public interface ILoginAttemptService
{
    Task<bool> IsLockedOutAsync(string email);
    Task RegisterFailedAttemptAsync(string email);
    Task ResetAttemptsAsync(string email);
    Task<TimeSpan?> GetLockoutRemainingAsync(string email);
}
