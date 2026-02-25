using System;

namespace AkyildizYonetim.Domain.Entities;

public class UserPushSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    
    // Navigation property
    public User? User { get; set; }
}
