namespace AkyildizYonetim.Domain.Entities;

public enum UserRole
{
    Admin,      // Yönetici
    Owner,      // Mal Sahibi
    Tenant,     // Kiracı
    Observer    // Gözlemci
}

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    // Navigation
    public virtual Owner? Owner { get; set; }
    public virtual Tenant? Tenant { get; set; }
} 