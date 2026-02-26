using FluentValidation;

namespace AkyildizYonetim.Application.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Güncellenecek ödeme ID'si boş olamaz.");

        RuleFor(v => v.Amount)
            .GreaterThan(0).WithMessage("Ödeme tutarı 0'dan büyük olmalıdır.");

        RuleFor(v => v.PaymentDate)
            .NotEmpty().WithMessage("Ödeme tarihi boş olamaz.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");
            
        RuleFor(v => v.ReceiptNumber)
            .MaximumLength(100).WithMessage("Makbuz no 100 karakterden uzun olamaz.");
    }
}
