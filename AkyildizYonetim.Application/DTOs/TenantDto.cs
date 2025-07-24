namespace AkyildizYonetim.Application.DTOs;

using System;
using System.Collections.Generic;

public class TenantDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public string ContactPersonEmail { get; set; } = string.Empty;
    public decimal MonthlyAidat { get; set; }
    public decimal ElectricityRate { get; set; }
    public decimal WaterRate { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FlatInfoDto> Flats { get; set; } = new List<FlatInfoDto>();
}

public class FlatInfoDto
{
    public Guid Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal UnitArea { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
} 