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

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("Kimlik/vergi numarası gereklidir.")
            .MaximumLength(20).WithMessage("Kimlik/vergi numarası 20 karakterden uzun olamaz.");

        RuleFor(x => x.ContactPersonName)
            .NotEmpty().WithMessage("İletişim kişisi adı gereklidir.")
            .MaximumLength(100).WithMessage("İletişim kişisi adı 100 karakterden uzun olamaz.");

        RuleFor(x => x.ContactPersonPhone)
            .NotEmpty().WithMessage("İletişim kişisi telefonu gereklidir.")
            .MaximumLength(20).WithMessage("İletişim kişisi telefonu 20 karakterden uzun olamaz.");

        RuleFor(x => x.ContactPersonEmail)
            .NotEmpty().WithMessage("İletişim kişisi e-postası gereklidir.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(100).WithMessage("İletişim kişisi e-postası 100 karakterden uzun olamaz.");

        // Lokasyon validasyonu - Floor veya FlatId'den en az biri olmalı
        RuleFor(x => x)
            .Must(command => command.FloorNumber.HasValue || command.FlatId.HasValue)
            .WithMessage("Kat seçimi veya ünite seçimi yapılmalıdır.");

        RuleFor(x => x.FloorNumber)
            .InclusiveBetween(1, 100).When(x => x.FloorNumber.HasValue)
            .WithMessage("Kat numarası 1-100 arasında olmalıdır.");

        RuleFor(x => x.FlatId)
            .NotEqual(Guid.Empty).When(x => x.FlatId.HasValue)
            .WithMessage("Geçerli bir ünite ID'si giriniz.");

        RuleFor(x => x.MonthlyAidat)
            .GreaterThanOrEqualTo(0).WithMessage("Aylık aidat negatif olamaz.");
    }
}