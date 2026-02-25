using System;

namespace AkyildizYonetim.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "Debt", "Payment", "Announcement"
    public bool IsRead { get; set; }
    public Guid? RelatedEntityId { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}
