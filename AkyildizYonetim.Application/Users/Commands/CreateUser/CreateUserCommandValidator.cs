using AkyildizYonetim.Domain.Entities;
using FluentValidation;

namespace AkyildizYonetim.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur.")
            .MaximumLength(100).WithMessage("Ad 100 karakterden uzun olamaz.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur.")
            .MaximumLength(100).WithMessage("Soyad 100 karakterden uzun olamaz.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçerli bir rol seçiniz.");

        // Rule: Tenant role must have a TenantId
        RuleFor(x => x.TenantId)
            .NotEmpty().When(x => x.Role == UserRole.Tenant)
            .WithMessage("Kiracı rolü için bir firma seçilmesi zorunludur.");

        // Rule: Non-tenant roles must NOT have a TenantId
        RuleFor(x => x.TenantId)
            .Empty().When(x => x.Role != UserRole.Tenant)
            .WithMessage("Sadece Kiracı rolü bir firma ile ilişkilendirilebilir.");

        // Manager / Admin cannot be linked to OwnerId if that was the intent, 
        // but current logic only uses TenantId for CompanyId mapping.
    }
}
