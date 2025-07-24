using FluentValidation;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;

public class UseAdvanceAccountCommandValidator : AbstractValidator<UseAdvanceAccountCommand>
{
    public UseAdvanceAccountCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Kiracı ID'si gereklidir.");

        RuleFor(x => x.DebtPayments)
            .NotEmpty()
            .WithMessage("En az bir borç ödemesi belirtilmelidir.");

        RuleForEach(x => x.DebtPayments)
            .SetValidator(new DebtPaymentRequestValidator());

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}

public class DebtPaymentRequestValidator : AbstractValidator<DebtPaymentRequest>
{
    public DebtPaymentRequestValidator()
    {
        RuleFor(x => x.DebtId)
            .NotEmpty()
            .WithMessage("Borç ID'si gereklidir.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Ödeme tutarı 0'dan büyük olmalıdır.");
    }
} 