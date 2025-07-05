using System.ComponentModel.DataAnnotations;

namespace AkyildizYonetim.Domain.Entities;

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Payment,
    DebtAllocation,
    AdvanceAccountUpdate
}

public enum AuditEntityType
{
    Payment,
    UtilityDebt,
    AdvanceAccount,
    PaymentDebt,
    Tenant,
    Owner
}

public class AuditLog : BaseEntity
{
    [Required]
    public AuditAction Action { get; set; }
    
    [Required]
    public AuditEntityType EntityType { get; set; }
    
    [Required]
    public Guid EntityId { get; set; }
    
    public string? EntityName { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? UserName { get; set; }
    
    public string? Description { get; set; }
    
    public string? OldValues { get; set; } // JSON formatında eski değerler
    
    public string? NewValues { get; set; } // JSON formatında yeni değerler
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
} 