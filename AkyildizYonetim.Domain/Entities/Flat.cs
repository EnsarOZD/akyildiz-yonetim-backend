namespace AkyildizYonetim.Domain.Entities;

public class Flat : BaseEntity
{
    public string Number { get; set; } = string.Empty; // Daire numarası
    public int Floor { get; set; } // Kat
    public Guid OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public bool IsActive { get; set; } = true;
    

    // Navigation properties
    public virtual Owner Owner { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
} 