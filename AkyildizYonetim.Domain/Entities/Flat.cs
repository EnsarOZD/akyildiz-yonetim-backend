namespace AkyildizYonetim.Domain.Entities;

public class Flat : BaseEntity
{
    public string Number { get; set; } = string.Empty; // Daire numarası
    public int Floor { get; set; } // Kat
    public Guid OwnerId { get; set; }
    public Guid? TenantId { get; set; }

    // Navigation properties
    public virtual Owner Owner { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
} 