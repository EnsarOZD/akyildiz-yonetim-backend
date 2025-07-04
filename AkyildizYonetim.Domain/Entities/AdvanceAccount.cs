using System.ComponentModel.DataAnnotations;

namespace AkyildizYonetim.Domain.Entities;

public class AdvanceAccount : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public decimal Balance { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
} 