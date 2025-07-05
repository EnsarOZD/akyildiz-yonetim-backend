using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands.CreateMeterReading;

public record CreateMeterReadingCommand : IRequest<Result<Guid>>
{
    public Guid FlatId { get; init; }
    public MeterType Type { get; init; }
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public decimal ReadingValue { get; init; }
    public decimal Consumption { get; init; }
    public DateTime ReadingDate { get; init; }
    public string? Note { get; init; }
}

public class CreateMeterReadingCommandValidator : AbstractValidator<CreateMeterReadingCommand>
{
    public CreateMeterReadingCommandValidator()
    {
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

public class CreateMeterReadingCommandHandler : IRequestHandler<CreateMeterReadingCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateMeterReadingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateMeterReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Daire kontrolü
            var flat = await _context.Flats
                .FirstOrDefaultAsync(f => f.Id == request.FlatId && !f.IsDeleted, cancellationToken);

            if (flat == null)
                return Result<Guid>.Failure("Seçilen daire bulunamadı");

            // Aynı dönem için aynı tip sayaç okuması var mı kontrol et
            var existingReading = await _context.MeterReadings
                .FirstOrDefaultAsync(mr => 
                    mr.FlatId == request.FlatId && 
                    mr.Type == request.Type && 
                    mr.PeriodYear == request.PeriodYear && 
                    mr.PeriodMonth == request.PeriodMonth && 
                    !mr.IsDeleted, 
                    cancellationToken);

            if (existingReading != null)
                return Result<Guid>.Failure($"Bu dönem için {request.Type} sayacı okuması zaten mevcut");

            var meterReading = new MeterReading
            {
                Id = Guid.NewGuid(),
                FlatId = request.FlatId,
                Type = request.Type,
                PeriodYear = request.PeriodYear,
                PeriodMonth = request.PeriodMonth,
                ReadingValue = request.ReadingValue,
                Consumption = request.Consumption,
                ReadingDate = request.ReadingDate,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            };

            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(meterReading.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Sayaç okuması oluşturulurken hata oluştu: {ex.Message}");
        }
    }
} 