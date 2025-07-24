using FluentValidation;

namespace AkyildizYonetim.Application.AdvanceAccounts.Commands.UpdateAdvanceAccount;

public class UpdateAdvanceAccountCommandValidator : AbstractValidator<UpdateAdvanceAccountCommand>
{
    public UpdateAdvanceAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Avans hesabı ID'si gereklidir.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Kiracı ID'si gereklidir.");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Bakiye 0'dan küçük olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
} 