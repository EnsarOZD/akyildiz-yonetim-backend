namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;

public record DebtPaymentRequest
{
    public Guid DebtId { get; init; }
    public decimal Amount { get; init; }
} 