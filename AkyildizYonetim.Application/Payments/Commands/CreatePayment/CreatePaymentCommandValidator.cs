using FluentValidation;

namespace AkyildizYonetim.Application.Payments.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(v => v.Amount)
            .GreaterThan(0).WithMessage("Ödeme tutarı 0'dan büyük olmalıdır.");

        RuleFor(v => v.PaymentDate)
            .NotEmpty().WithMessage("Ödeme tarihi boş olamaz.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Ödeme tarihi gelecek bir tarih olamaz.");

        RuleFor(v => v.TenantId)
            .NotEmpty().When(v => !v.OwnerId.HasValue).WithMessage("Kiracı veya Mal Sahibi seçilmelidir.");

        RuleFor(v => v.OwnerId)
            .NotEmpty().When(v => !v.TenantId.HasValue).WithMessage("Kiracı veya Mal Sahibi seçilmelidir.");
            
        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");
            
        RuleFor(v => v.ReceiptNumber)
            .MaximumLength(100).WithMessage("Makbuz no 100 karakterden uzun olamaz.");
    }
}
