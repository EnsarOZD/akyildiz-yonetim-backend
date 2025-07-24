using System.ComponentModel.DataAnnotations;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UpdateAdvanceAccount;

public class UpdateAdvanceAccountRequest
{
    [Required(ErrorMessage = "Kiracı ID'si gereklidir.")]
    public Guid TenantId { get; set; }
    
    [Required(ErrorMessage = "Bakiye gereklidir.")]
    [Range(0, double.MaxValue, ErrorMessage = "Bakiye 0'dan küçük olamaz.")]
    public decimal Balance { get; set; }
    
    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
} 