using AkyildizYonetim.Domain.Entities.Enums;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Application.DTOs
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactPersonPhone { get; set; } = string.Empty;
        public string ContactPersonEmail { get; set; } = string.Empty;
        public decimal MonthlyAidat { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal AdvanceBalance { get; set; }
        public List<TenantFlatInfoDto> Flats { get; set; } = new List<TenantFlatInfoDto>();
    }

    public class TenantFlatInfoDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? FloorNumber { get; set; }
        public UnitType Type { get; set; }
        public decimal UnitArea { get; set; }
        public bool IsOccupied { get; set; }
    }
}
