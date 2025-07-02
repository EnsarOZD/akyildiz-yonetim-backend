using System;

namespace AkyildizYonetim.Domain.Entities;

public class AidatDefinition
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public decimal VatIncludedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
} 