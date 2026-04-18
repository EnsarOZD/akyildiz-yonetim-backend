namespace AkyildizYonetim.Domain.Entities;

public enum ServiceRequestStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum ServiceRequestCategory
{
    Maintenance,
    Cleaning,
    Noise,
    Security,
    Other
}

public class ServiceRequest : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Open;
    public ServiceRequestCategory Category { get; set; } = ServiceRequestCategory.Other;
    public string? AttachmentUrl { get; set; }
    public string? AdminNote { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Guid? AssignedPersonnelId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OwnerId { get; set; }

    public virtual User? AssignedPersonnel { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public virtual Owner? Owner { get; set; }
}
