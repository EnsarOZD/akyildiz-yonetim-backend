using FluentValidation;

namespace AkyildizYonetim.Application.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık 200 karakterden uzun olamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Geçersiz gider tipi.");

        RuleFor(x => x.ExpenseDate)
            .NotEmpty().WithMessage("Tarih boş olamaz.")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("Tarih gelecekte olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakterden uzun olamaz.");

        RuleFor(x => x.ReceiptNumber)
            .MaximumLength(50).WithMessage("Makbuz numarası 50 karakterden uzun olamaz.");
    }
} 