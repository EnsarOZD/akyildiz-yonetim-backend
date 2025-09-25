using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands.BulkUpsertMeterReadings;

public record BulkUpsertMeterReadingsCommand : IRequest<Result<int>>
{
    public List<BulkUpsertMeterReadingItem> Items { get; init; } = new();
}

public record BulkUpsertMeterReadingItem
{
    public Guid? Id { get; init; } // null ise create, dolu ise update
    public Guid FlatId { get; init; }
    public MeterType Type { get; init; }
    public int PeriodYear { get; init; }
    public int PeriodMonth { get; init; }
    public decimal ReadingValue { get; init; }
    public decimal? Consumption { get; init; } // null ise otomatik hesaplanır
    public DateTime ReadingDate { get; init; }
    public string? Note { get; init; }
}

public class BulkUpsertMeterReadingsCommandValidator : AbstractValidator<BulkUpsertMeterReadingsCommand>
{
    public BulkUpsertMeterReadingsCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("En az bir sayaç okuması gereklidir")
            .Must(items => items.Count <= 100).WithMessage("Bir seferde en fazla 100 sayaç okuması işlenebilir");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.FlatId)
                .NotEmpty().WithMessage("Daire seçimi zorunludur");

            item.RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Geçerli bir sayaç tipi seçiniz");

            item.RuleFor(x => x.PeriodYear)
                .GreaterThan(2000).WithMessage("Geçerli bir yıl giriniz")
                .LessThan(2100).WithMessage("Geçerli bir yıl giriniz");

            item.RuleFor(x => x.PeriodMonth)
                .GreaterThan(0).WithMessage("Geçerli bir ay giriniz")
                .LessThan(13).WithMessage("Geçerli bir ay giriniz");

            item.RuleFor(x => x.ReadingValue)
                .GreaterThanOrEqualTo(0).WithMessage("Sayaç değeri 0'dan küçük olamaz");

            item.RuleFor(x => x.Consumption)
                .GreaterThanOrEqualTo(0).WithMessage("Tüketim değeri 0'dan küçük olamaz")
                .When(x => x.Consumption.HasValue);

            item.RuleFor(x => x.ReadingDate)
                .NotEmpty().WithMessage("Okuma tarihi zorunludur");

            item.RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Not 500 karakterden uzun olamaz");
        });
    }
}

public class BulkUpsertMeterReadingsCommandHandler : IRequestHandler<BulkUpsertMeterReadingsCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public BulkUpsertMeterReadingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(BulkUpsertMeterReadingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var affectedCount = 0;

            foreach (var item in request.Items)
            {
                if (item.Id.HasValue)
                {
                    // Update existing meter reading
                    var existingReading = await _context.MeterReadings
                        .FirstOrDefaultAsync(mr => mr.Id == item.Id.Value && !mr.IsDeleted, cancellationToken);

                    if (existingReading != null)
                    {
                        // Aynı dönem için aynı tip sayaç okuması var mı kontrol et (kendisi hariç)
                        var duplicateReading = await _context.MeterReadings
                            .FirstOrDefaultAsync(mr =>
                                mr.Id != item.Id.Value &&
                                mr.FlatId == item.FlatId &&
                                mr.Type == item.Type &&
                                mr.PeriodYear == item.PeriodYear &&
                                mr.PeriodMonth == item.PeriodMonth &&
                                !mr.IsDeleted,
                                cancellationToken);

                        if (duplicateReading != null)
                            continue; // Skip this item

                        // Önceki okumayı bul ve tüketimi hesapla
                        var prev = await _context.MeterReadings
                            .Where(mr => !mr.IsDeleted
                                         && mr.FlatId == item.FlatId
                                         && mr.Type == item.Type
                                         && (mr.PeriodYear < item.PeriodYear
                                             || (mr.PeriodYear == item.PeriodYear && mr.PeriodMonth < item.PeriodMonth))
                                         && mr.Id != item.Id.Value)
                            .OrderByDescending(mr => mr.PeriodYear)
                            .ThenByDescending(mr => mr.PeriodMonth)
                            .FirstOrDefaultAsync(cancellationToken);

                        var prevValue = prev?.ReadingValue ?? 0m;
                        var consumption = item.Consumption ?? Math.Max(0m, item.ReadingValue - prevValue);

                        existingReading.FlatId = item.FlatId;
                        existingReading.Type = item.Type;
                        existingReading.PeriodYear = item.PeriodYear;
                        existingReading.PeriodMonth = item.PeriodMonth;
                        existingReading.ReadingValue = item.ReadingValue;
                        existingReading.Consumption = consumption;
                        existingReading.ReadingDate = item.ReadingDate;
                        existingReading.Note = item.Note;
                        existingReading.UpdatedAt = DateTime.UtcNow;

                        affectedCount++;
                    }
                }
                else
                {
                    // Create new meter reading
                    // Aynı dönem için aynı tip sayaç okuması var mı kontrol et
                    var existingReading = await _context.MeterReadings
                        .FirstOrDefaultAsync(mr =>
                            mr.FlatId == item.FlatId &&
                            mr.Type == item.Type &&
                            mr.PeriodYear == item.PeriodYear &&
                            mr.PeriodMonth == item.PeriodMonth &&
                            !mr.IsDeleted,
                            cancellationToken);

                    if (existingReading != null)
                        continue; // Skip this item

                    // Önceki okumayı bul ve tüketimi hesapla
                    var prev = await _context.MeterReadings
                        .Where(mr => !mr.IsDeleted
                                     && mr.FlatId == item.FlatId
                                     && mr.Type == item.Type
                                     && (mr.PeriodYear < item.PeriodYear
                                         || (mr.PeriodYear == item.PeriodYear && mr.PeriodMonth < item.PeriodMonth)))
                        .OrderByDescending(mr => mr.PeriodYear)
                        .ThenByDescending(mr => mr.PeriodMonth)
                        .FirstOrDefaultAsync(cancellationToken);

                    var prevValue = prev?.ReadingValue ?? 0m;
                    var consumption = item.Consumption ?? Math.Max(0m, item.ReadingValue - prevValue);

                    var newReading = new MeterReading
                    {
                        Id = Guid.NewGuid(),
                        FlatId = item.FlatId,
                        Type = item.Type,
                        PeriodYear = item.PeriodYear,
                        PeriodMonth = item.PeriodMonth,
                        ReadingValue = item.ReadingValue,
                        Consumption = consumption,
                        ReadingDate = item.ReadingDate,
                        Note = item.Note,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.MeterReadings.Add(newReading);
                    affectedCount++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(affectedCount);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Toplu sayaç okuması işlenirken hata oluştu: {ex.Message}");
        }
    }
}

