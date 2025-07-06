namespace AkyildizYonetim.Domain.Entities;

public class Tenant : BaseEntity
{
    // İş Yeri Bilgileri
    public string CompanyName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty; // Ticaret, Hizmet, Üretim vs.
    public string TaxNumber { get; set; } = string.Empty;
    
    // İletişim Kişisi Bilgileri
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public string ContactPersonEmail { get; set; } = string.Empty;
    
    // Aidat ve Borç Yönetimi
    public decimal MonthlyAidat { get; set; } // Aylık aidat
    public decimal ElectricityRate { get; set; } // Elektrik tarifesi (kWh başına)
    public decimal WaterRate { get; set; } // Su tarifesi (m³ başına)
    public bool IsActive { get; set; } = true;
    
    // Sözleşme Bilgileri (Opsiyonel)
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Flat> Flats { get; set; } = new List<Flat>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
} 