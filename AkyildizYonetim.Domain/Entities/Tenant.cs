namespace AkyildizYonetim.Domain.Entities;

public class Tenant : BaseEntity
{
    // İş Yeri Bilgileri
    public string CompanyName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty; // Ticaret, Hizmet, Üretim vs.
    public string IdentityNumber { get; set; } = string.Empty; // TC Kimlik No veya Vergi No
    
    // İletişim Kişisi Bilgileri
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public string ContactPersonEmail { get; set; } = string.Empty;
    
    // Aidat ve Borç Yönetimi
    public decimal MonthlyAidat { get; set; } // Aylık aidat
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Flat> Flats { get; set; } = new List<Flat>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<AdvanceAccount> AdvanceAccounts { get; set; } = new List<AdvanceAccount>();
    public virtual ICollection<PaymentDebt> PaymentDebts { get; set; } = new List<PaymentDebt>();
} 