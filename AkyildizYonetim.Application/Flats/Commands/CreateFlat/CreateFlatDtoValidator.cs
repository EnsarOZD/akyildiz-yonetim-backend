using AkyildizYonetim.Application.DTOs;
using FluentValidation;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

public class CreateFlatDtoValidator : AbstractValidator<CreateFlatDto>
{
	public CreateFlatDtoValidator()
	{
		// Temel
		RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
		RuleFor(x => x.UnitArea).GreaterThanOrEqualTo(0);
		RuleFor(x => x.MonthlyRent).GreaterThanOrEqualTo(0);
		RuleFor(x => x.Description).MaximumLength(500);

		// Parking
		When(x => x.Type == UnitType.Parking, () =>
		{
			RuleFor(x => x.FloorNumber).Must(n => n == null)
				.WithMessage("Parking için FloorNumber null olmalıdır.");
			RuleFor(x => x.GroupKey).Null();
			RuleFor(x => x.Section).Null();
			RuleFor(x => x.GroupStrategy).Equal(GroupStrategy.None);

			// Eğer DTO'ların nullable ise:
			RuleFor(x => x.OwnerId).Must(id => id == null)
				.WithMessage("Parking için OwnerId boş olmalıdır.");
			RuleFor(x => x.TenantId).Must(id => id == null)
				.WithMessage("Parking için TenantId boş olmalıdır.");
		});

		// Entry (Giriş: GA/GB)
		When(x => x.Type == UnitType.Entry, () =>
		{
			RuleFor(x => x.FloorNumber).Equal(0)
				.WithMessage("Entry (giriş) için FloorNumber 0 olmalıdır.");

			RuleFor(x => x.GroupStrategy).Equal(GroupStrategy.SplitIfMultiple)
				.WithMessage("Entry için GroupStrategy = SplitIfMultiple olmalıdır.");

			RuleFor(x => x.GroupKey).NotEmpty()
				.WithMessage("Entry için GroupKey zorunludur (örn. 'G').");

			RuleFor(x => x.Section).NotEmpty()
				.Must(s => s == "A" || s == "B")
				.WithMessage("Entry için Section sadece 'A' veya 'B' olabilir.");
		});

		// Split grupları (ör: 3A/3B)
		When(x => x.GroupStrategy == GroupStrategy.SplitIfMultiple && x.Type != UnitType.Entry, () =>
		{
			RuleFor(x => x.GroupKey).NotEmpty()
				.WithMessage("Split gruplarında GroupKey zorunludur (örn. '3').");

			RuleFor(x => x.Section).NotEmpty()
				.Must(s => s == "A" || s == "B")
				.WithMessage("Split gruplarında Section sadece 'A' veya 'B' olabilir.");
		});

		// Split olmayan (normal) katlar
		When(x => x.GroupStrategy != GroupStrategy.SplitIfMultiple && x.Type == UnitType.Floor, () =>
		{
			RuleFor(x => x.GroupKey).Null()
				.WithMessage("Normal katlarda GroupKey boş olmalıdır.");
			RuleFor(x => x.Section).Null()
				.WithMessage("Normal katlarda Section boş olmalıdır.");
		});

		// Floor: 0 olamaz (0 zaten Entry)
		When(x => x.Type == UnitType.Floor, () =>
		{
			RuleFor(x => x.FloorNumber).NotNull()
				.WithMessage("Floor için FloorNumber zorunludur.")
				.Must(n => n != 0)
				.WithMessage("Floor için FloorNumber 0 olamaz (0 giriş katıdır).");
		});

		// (Opsiyonel) Code formatı için basit bir kural istiyorsan:
		// RuleFor(x => x.Code).Matches(@"^[A-Z0-9\-]+$").WithMessage("Code sadece A-Z, 0-9 ve '-' içerebilir.");
	}
}

public class UpdateFlatDtoValidator : AbstractValidator<UpdateFlatDto>
{
	public UpdateFlatDtoValidator()
	{
		Include(new CreateFlatDtoValidator());
		RuleFor(x => x.Id).NotEmpty();
	}
}
