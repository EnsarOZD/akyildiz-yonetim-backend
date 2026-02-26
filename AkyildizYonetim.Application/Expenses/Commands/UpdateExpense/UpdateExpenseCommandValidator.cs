using FluentValidation;

namespace AkyildizYonetim.Application.Expenses.Commands.UpdateExpense;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Güncellenecek gider ID'si boş olamaz.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Gider başlığı boş olamaz.")
            .MaximumLength(200).WithMessage("Gider başlığı 200 karakterden uzun olamaz.");

        RuleFor(v => v.Amount)
            .GreaterThan(0).WithMessage("Gider tutarı 0'dan büyük olmalıdır.");

        RuleFor(v => v.ExpenseDate)
            .NotEmpty().WithMessage("Gider tarihi boş olamaz.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakterden uzun olamaz.");
    }
}
