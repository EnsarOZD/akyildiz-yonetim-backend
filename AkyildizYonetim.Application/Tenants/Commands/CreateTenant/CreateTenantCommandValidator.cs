using FluentValidation;

namespace AkyildizYonetim.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Şirket adı gereklidir.")
            .MaximumLength(200).WithMessage("Şirket adı 200 karakterden uzun olamaz.");

        RuleFor(x => x.BusinessType)
            .NotEmpty().WithMessage("İş türü gereklidir.")
            .MaximumLength(100).WithMessage("İş türü 100 karakterden uzun olamaz.");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Vergi numarası gereklidir.")
            .MaximumLength(20).WithMessage("Vergi numarası 20 karakterden uzun olamaz.");

        RuleFor(x => x.ContactPersonName)
            .NotEmpty().WithMessage("İletişim kişisi adı gereklidir.")
            .MaximumLength(100).WithMessage("İletişim kişisi adı 100 karakterden uzun olamaz.");

        RuleFor(x => x.ContactPersonPhone)
            .NotEmpty().WithMessage("İletişim kişisi telefonu gereklidir.")
            .MaximumLength(20).WithMessage("İletişim kişisi telefonu 20 karakterden uzun olamaz.");

        RuleFor(x => x.ContactPersonEmail)
            .NotEmpty().WithMessage("İletişim kişisi e-postası gereklidir.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(255).WithMessage("İletişim kişisi e-postası 255 karakterden uzun olamaz.");

        RuleFor(x => x.FlatId)
            .NotEmpty().WithMessage("Daire seçimi gereklidir.");

        RuleFor(x => x.MonthlyAidat)
            .GreaterThan(0).WithMessage("Aylık aidat 0'dan büyük olmalıdır.");
    }
} 