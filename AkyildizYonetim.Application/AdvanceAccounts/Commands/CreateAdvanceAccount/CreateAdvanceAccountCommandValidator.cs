using FluentValidation;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.CreateAdvanceAccount;

public class CreateAdvanceAccountCommandValidator : AbstractValidator<CreateAdvanceAccountCommand>
{
    public CreateAdvanceAccountCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Kiracı ID'si gereklidir.");

        RuleFor(x => x.Balance)
            .GreaterThan(0)
            .WithMessage("Bakiye 0'dan büyük olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
} 