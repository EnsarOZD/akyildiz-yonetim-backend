namespace AkyildizYonetim.Domain.Entities;

public class Owner : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public decimal MonthlyDues { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public virtual ICollection<Flat> Flats { get; set; } = new List<Flat>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
} 