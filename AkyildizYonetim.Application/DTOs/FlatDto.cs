namespace AkyildizYonetim.Application.DTOs;

using System;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

public class FlatDto
{
	public Guid Id { get; set; }

	// Yeni model alanları
	public string Code { get; set; } = string.Empty;        // "7", "3A", "GA", "OTOPARK"
	public int? FloorNumber { get; set; }                   // OTOPARK için null olabilir
	public string? Section { get; set; }                    // "A" | "B" | null
	public UnitType Type { get; set; }                      // Floor | Entry | Parking
	public string? GroupKey { get; set; }                   // "3", "G" veya null
	public GroupStrategy GroupStrategy { get; set; }        // None | SplitIfMultiple

	// Durum & ilişkiler
	public bool IsActive { get; set; } = true;
	public bool IsOccupied { get; set; } = false;
	public Guid? OwnerId { get; set; }
	public Guid? TenantId { get; set; }

	// Bilgi alanları
	public decimal UnitArea { get; set; }
	public decimal MonthlyRent { get; set; } = 0;
	public string Description { get; set; } = string.Empty;

	// Görünüm amaçlı (DB’ye yazmıyoruz)
	public decimal? EffectiveShare { get; set; }

	// Sistem
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// Legacy alanlar (geçiş dönemi için)
	public string Number { get; set; } = string.Empty;      // eski
	public string UnitNumber { get; set; } = string.Empty;  // eski
	public string ApartmentNumber { get; set; } = string.Empty; // eski
}

// Kısa Flat bilgileri için (liste görünümünde kullanılacak)
public class FlatSummaryDto
{
	public Guid Id { get; set; }
	public string Code { get; set; } = string.Empty;        // liste görünümünde tek bakışta yeterli
	public int? FloorNumber { get; set; }
	public UnitType Type { get; set; }
	public bool IsOccupied { get; set; }
	public bool IsActive { get; set; }
	public decimal UnitArea { get; set; }
	public decimal? EffectiveShare { get; set; }
	public string OwnerName { get; set; } = string.Empty;
	public string? TenantCompanyName { get; set; }
}

// Flat oluşturma için
public class CreateFlatDto
{
	// Zorunlu
	public string Code { get; set; } = string.Empty;
	public UnitType Type { get; set; } = UnitType.Floor;

	// Opsiyonel/koşullu
	public int? FloorNumber { get; set; }                   // Floor için zorunlu, Parking için null
	public string? Section { get; set; }                    // A/B sadece split gruplarında
	public string? GroupKey { get; set; }                   // "3"/"G" sadece split gruplarında
	public GroupStrategy GroupStrategy { get; set; } = GroupStrategy.None;

	public bool IsActive { get; set; } = true;
	public bool IsOccupied { get; set; } = false;
	public Guid? OwnerId { get; set; }
	public Guid? TenantId { get; set; }                     // genelde null başlar

	public decimal UnitArea { get; set; }
	public decimal MonthlyRent { get; set; } = 0;
	public string Description { get; set; } = string.Empty;

	// Legacy isteğe bağlı
	public string Number { get; set; } = string.Empty;
	public string UnitNumber { get; set; } = string.Empty;
	public string ApartmentNumber { get; set; } = string.Empty;
}

// Flat güncelleme için
public class UpdateFlatDto:CreateFlatDto
{
	public Guid Id { get; set; }
} 