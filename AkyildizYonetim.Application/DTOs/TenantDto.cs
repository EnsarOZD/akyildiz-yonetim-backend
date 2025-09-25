using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums; // UnitType için

public class TenantDto
{
	public Guid Id { get; set; }
	// Ýþ Yeri Bilgileri
	public string CompanyName { get; set; } = string.Empty;
	public string BusinessType { get; set; } = string.Empty;
	public string CompanyType { get; set; } = string.Empty;
	public string IdentityNumber { get; set; } = string.Empty;
	// Ýletiþim
	public string ContactPersonName { get; set; } = string.Empty;
	public string ContactPersonPhone { get; set; } = string.Empty;
	public string ContactPersonEmail { get; set; } = string.Empty;
	// Aidat
	public decimal MonthlyAidat { get; set; }
	// Sözleþme
	public DateTime? ContractStartDate { get; set; }
	public DateTime? ContractEndDate { get; set; }
	// Sistem
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// Güncel Flat özetleri
	public List<TenantFlatInfoDto> Flats { get; set; } = new();
}

public class TenantFlatInfoDto
{
	public Guid Id { get; set; }
	public string Code { get; set; } = string.Empty;   // "3A", "OTOPARK", ...
	public int? FloorNumber { get; set; }              // OTOPARK için null olabilir
	public UnitType Type { get; set; }                 // Floor | Entry | Parking
	public decimal UnitArea { get; set; }
	public bool IsOccupied { get; set; }
}
