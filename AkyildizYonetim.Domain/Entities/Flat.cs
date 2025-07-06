namespace AkyildizYonetim.Domain.Entities;

public class Flat : BaseEntity
{
    public string Number { get; set; } = string.Empty; // Daire numarası
    public string UnitNumber { get; set; } = string.Empty; // A-101, B-205 gibi ünite numarası
    public int Floor { get; set; } // Kat
    public decimal UnitArea { get; set; } // m² alan
    public Guid OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOccupied { get; set; } = false; // Dolu/Boş durumu
    public string Category { get; set; } = "Normal"; // Normal, OrtakAlan, Mescit, Otopark gibi
    public int ShareCount { get; set; } = 1; // Ortak paylaşımda hisse sayısı (ör: -3/-4 için 2, diğerleri için 1)
    
    // İş Hanı Özel Alanları
    public string BusinessType { get; set; } = string.Empty; // Bu ünitede yapılan iş türü
    public decimal MonthlyRent { get; set; } = 0; // Aylık kira (varsa)
    public string Description { get; set; } = string.Empty; // Ünite açıklaması

    // Navigation properties
    public virtual Owner Owner { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
} 