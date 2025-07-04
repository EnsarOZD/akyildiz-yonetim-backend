using System;

namespace AkyildizYonetim.Domain.Entities;

public class AidatDefinition : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public decimal VatIncludedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
} 