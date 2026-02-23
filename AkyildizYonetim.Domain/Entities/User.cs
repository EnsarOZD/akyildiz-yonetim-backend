namespace AkyildizYonetim.Domain.Entities;

public enum UserRole
{
    Admin,      // Sistem Yöneticisi
    Manager,    // Yönetici (Müdür)
    Owner,      // Mal Sahibi
    Tenant,     // Kiracı
    Observer,   // Gözlemci (Avukat vb.)
    DataEntry   // Veri Giriş Sorumlusu
}

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PasswordResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    // Navigation
    public virtual Owner? Owner { get; set; }
    public virtual Tenant? Tenant { get; set; }
} 