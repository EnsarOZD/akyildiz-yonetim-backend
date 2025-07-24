using System.ComponentModel.DataAnnotations;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;

public class UseAdvanceAccountRequest
{
    [Required(ErrorMessage = "Kiracı ID'si gereklidir.")]
    public Guid TenantId { get; set; }
    
    [Required(ErrorMessage = "Borç ödemeleri gereklidir.")]
    [MinLength(1, ErrorMessage = "En az bir borç ödemesi belirtilmelidir.")]
    public List<DebtPaymentRequest> DebtPayments { get; set; } = new();
    
    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }
} 