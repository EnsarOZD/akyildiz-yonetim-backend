namespace AkyildizYonetim.Domain.Entities;

public enum ExpenseType
{
    Electricity,
    Water,
    Gas,
    Maintenance,
    Cleaning,
    Security,
    Other,
    FoodAndBeverage,
    Salary,
    Tax
}

public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseType Type { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    
    // Foreign keys
    public Guid? OwnerId { get; set; }
    
    // Navigation properties
    public virtual Owner? Owner { get; set; }
} 