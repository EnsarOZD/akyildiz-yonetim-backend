using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands.UpdateMeterReading;

public record UpdateMeterReadingCommand : IRequest<Result>
{
	public Guid Id { get; init; }
	public Guid FlatId { get; init; }
	public MeterType Type { get; init; }
	public int PeriodYear { get; init; }
	public int PeriodMonth { get; init; }
	public decimal ReadingValue { get; init; }
	public decimal Consumption { get; init; }
	public DateTime ReadingDate { get; init; }
	public string? Note { get; init; }
}

public class UpdateMeterReadingCommandValidator : AbstractValidator<UpdateMeterReadingCommand>
{
	public UpdateMeterReadingCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Sayaç okuması ID'si zorunludur");

		RuleFor(x => x.FlatId)
			.NotEmpty().WithMessage("Daire seçimi zorunludur");

		RuleFor(x => x.Type)
			.IsInEnum().WithMessage("Geçerli bir sayaç tipi seçiniz");

		RuleFor(x => x.PeriodYear)
			.GreaterThan(2000).WithMessage("Geçerli bir yıl giriniz")
			.LessThan(2100).WithMessage("Geçerli bir yıl giriniz");

		RuleFor(x => x.PeriodMonth)
			.GreaterThan(0).WithMessage("Geçerli bir ay giriniz")
			.LessThan(13).WithMessage("Geçerli bir ay giriniz");

		RuleFor(x => x.ReadingValue)
			.GreaterThanOrEqualTo(0).WithMessage("Sayaç değeri 0'dan küçük olamaz");

		RuleFor(x => x.Consumption)
			.GreaterThanOrEqualTo(0).WithMessage("Tüketim değeri 0'dan küçük olamaz");

		RuleFor(x => x.ReadingDate)
			.NotEmpty().WithMessage("Okuma tarihi zorunludur");

		RuleFor(x => x.Note)
			.MaximumLength(500).WithMessage("Not 500 karakterden uzun olamaz");
	}
}

public class UpdateMeterReadingCommandHandler : IRequestHandler<UpdateMeterReadingCommand, Result>
{
	private readonly IApplicationDbContext _context;

	public UpdateMeterReadingCommandHandler(IApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<Result> Handle(UpdateMeterReadingCommand request, CancellationToken cancellationToken)
	{
		try
		{
			var meterReading = await _context.MeterReadings
				.FirstOrDefaultAsync(mr => mr.Id == request.Id && !mr.IsDeleted, cancellationToken);

			if (meterReading == null)
				return Result.Failure("Sayaç okuması bulunamadı");

			// Daire kontrolü
			var flat = await _context.Flats
				.FirstOrDefaultAsync(f => f.Id == request.FlatId && !f.IsDeleted, cancellationToken);

			if (flat == null)
				return Result.Failure("Seçilen daire bulunamadı");

			// Aynı dönem için aynı tip sayaç okuması var mı kontrol et (kendisi hariç)
			var existingReading = await _context.MeterReadings
				.FirstOrDefaultAsync(mr =>
					mr.Id != request.Id &&
					mr.FlatId == request.FlatId &&
					mr.Type == request.Type &&
					mr.PeriodYear == request.PeriodYear &&
					mr.PeriodMonth == request.PeriodMonth &&
					!mr.IsDeleted,
					cancellationToken);

			if (existingReading != null)
				return Result.Failure($"Bu dönem için {request.Type} sayacı okuması zaten mevcut");

			var prev = await _context.MeterReadings
	.Where(mr => !mr.IsDeleted
				 && mr.FlatId == request.FlatId
				 && mr.Type == request.Type
				 && (mr.PeriodYear < request.PeriodYear
					 || (mr.PeriodYear == request.PeriodYear && mr.PeriodMonth < request.PeriodMonth))
				 && mr.Id != request.Id)
	.OrderByDescending(mr => mr.PeriodYear)
	.ThenByDescending(mr => mr.PeriodMonth)
	.FirstOrDefaultAsync(cancellationToken);

			var prevValue = prev?.ReadingValue ?? 0m;
			var consumption = request.Consumption > 0 ? request.Consumption : Math.Max(0m, request.ReadingValue - prevValue);
						
			meterReading.FlatId = request.FlatId;
			meterReading.Type = request.Type;
			meterReading.PeriodYear = request.PeriodYear;
			meterReading.PeriodMonth = request.PeriodMonth;
			meterReading.ReadingValue = request.ReadingValue;
			meterReading.Consumption = consumption;          // hesaplanmış tüketim
			meterReading.ReadingDate = request.ReadingDate;
			meterReading.Note = request.Note;
			meterReading.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync(cancellationToken);

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure($"Sayaç okuması güncellenirken hata oluştu: {ex.Message}");
		}
	}
}