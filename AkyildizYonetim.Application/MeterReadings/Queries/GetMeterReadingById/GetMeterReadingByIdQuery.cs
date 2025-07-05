using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.MeterReadings.DTOs;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Queries.GetMeterReadingById;

public record GetMeterReadingByIdQuery : IRequest<Result<MeterReadingDto>>
{
    public Guid Id { get; init; }
}

public class GetMeterReadingByIdQueryHandler : IRequestHandler<GetMeterReadingByIdQuery, Result<MeterReadingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMeterReadingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MeterReadingDto>> Handle(GetMeterReadingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var meterReading = await _context.MeterReadings
                .Include(mr => mr.Flat)
                .FirstOrDefaultAsync(mr => mr.Id == request.Id && !mr.IsDeleted, cancellationToken);

            if (meterReading == null)
                return Result<MeterReadingDto>.Failure("Sayaç okuması bulunamadı");

            var dto = new MeterReadingDto
            {
                Id = meterReading.Id,
                FlatId = meterReading.FlatId,
                FlatNumber = meterReading.Flat.ApartmentNumber,
                Type = meterReading.Type,
                PeriodYear = meterReading.PeriodYear,
                PeriodMonth = meterReading.PeriodMonth,
                ReadingValue = meterReading.ReadingValue,
                Consumption = meterReading.Consumption,
                ReadingDate = meterReading.ReadingDate,
                Note = meterReading.Note,
                CreatedAt = meterReading.CreatedAt,
                UpdatedAt = meterReading.UpdatedAt
            };

            return Result<MeterReadingDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<MeterReadingDto>.Failure($"Sayaç okuması alınırken hata oluştu: {ex.Message}");
        }
    }
} 