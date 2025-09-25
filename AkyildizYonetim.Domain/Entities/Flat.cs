using AkyildizYonetim.Domain.Entities.Enums;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkyildizYonetim.Domain.Entities;

public class Flat : BaseEntity
{
	// — Yeni/Net alanlar —
	public string Code { get; set; } = string.Empty;      // "7", "3A", "GA", "OTOPARK"
	public int? FloorNumber { get; set; }                 // OTOPARK için null bırakılabilir
	public string? Section { get; set; }                  // "A" | "B" | null
	public UnitType Type { get; set; } = UnitType.Floor;  // Floor | Entry | Parking

	// Bölünmüş kat grupları için (3A/3B -> "3", GA/GB -> "G")
	public string? GroupKey { get; set; }                 // "3", "G" veya null
	public GroupStrategy GroupStrategy { get; set; } = GroupStrategy.None;

	// Dolu/Boş durumu (hisse hesabında kullanacağız)
	public bool IsOccupied { get; set; } = false;

	// Görsel/rapor amaçlı: runtime’da hesaplanacak (DB’ye yazılmayacak)
	[NotMapped]
	public decimal? EffectiveShare { get; set; }          // 1, 0.5 veya 0

	// — Mevcut projeden gelen alanlar (kullanmaya devam edebiliriz) —
	public string Number { get; set; } = string.Empty;        // (Eski) Daire numarası
	public string UnitNumber { get; set; } = string.Empty;    // (Eski) "A-101" gibi
	public decimal UnitArea { get; set; }                     // m²
	public Guid? OwnerId { get; set; }                        // OTOPARK için nullable
	public Guid? TenantId { get; set; }
	public string ApartmentNumber { get; set; } = string.Empty;
	public bool IsActive { get; set; } = true;

	// Ortak paylaşımda “sabit” hisse tutma ihtiyacın kalmadı; dinamik hesaplayacağız.
	// Geriye dönük kırmamak için bırakıyorum ama kullanmayacağız:
	public int ShareCount { get; set; } = 1;

	// İş Hanı özel alanları
	public decimal MonthlyRent { get; set; } = 0;
	public string Description { get; set; } = string.Empty;

	// Nav props
	public virtual Owner? Owner { get; set; }
	public virtual Tenant? Tenant { get; set; }
} 