using FluentValidation;

namespace AkyildizYonetim.Application.AidatDefinitions.Commands.CreateAidatDefinition;

public class CreateAidatDefinitionCommandValidator : AbstractValidator<CreateAidatDefinitionCommand>
{
    public CreateAidatDefinitionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Kiracı ID gereklidir.");

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Birim adı gereklidir ve 50 karakterden uzun olamaz.");

        RuleFor(x => x.Year)
            .GreaterThan(2000)
            .LessThan(2100)
            .WithMessage("Yıl 2000-2100 arasında olmalıdır.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Tutar 0'dan büyük olmalıdır.");

        RuleFor(x => x.VatIncludedAmount)
            .GreaterThan(0)
            .WithMessage("KDV dahil tutar 0'dan büyük olmalıdır.");

        RuleFor(x => x.VatIncludedAmount)
            .GreaterThanOrEqualTo(x => x.Amount)
            .WithMessage("KDV dahil tutar, KDV hariç tutardan küçük olamaz.");
    }
} 