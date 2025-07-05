using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.MeterReadings.DTOs;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Queries.GetMeterReadings;

public record GetMeterReadingsQuery : IRequest<Result<List<MeterReadingDto>>>
{
    public Guid? FlatId { get; init; }
    public MeterType? Type { get; init; }
    public int? PeriodYear { get; init; }
    public int? PeriodMonth { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetMeterReadingsQueryHandler : IRequestHandler<GetMeterReadingsQuery, Result<List<MeterReadingDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMeterReadingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MeterReadingDto>>> Handle(GetMeterReadingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.MeterReadings
                .Include(mr => mr.Flat)
                .AsQueryable();

            // Filtreler
            if (request.FlatId.HasValue)
                query = query.Where(mr => mr.FlatId == request.FlatId.Value);

            if (request.Type.HasValue)
                query = query.Where(mr => mr.Type == request.Type.Value);

            if (request.PeriodYear.HasValue)
                query = query.Where(mr => mr.PeriodYear == request.PeriodYear.Value);

            if (request.PeriodMonth.HasValue)
                query = query.Where(mr => mr.PeriodMonth == request.PeriodMonth.Value);

            if (request.StartDate.HasValue)
                query = query.Where(mr => mr.ReadingDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(mr => mr.ReadingDate <= request.EndDate.Value);

            // Sıralama
            var meterReadings = await query
                .OrderByDescending(mr => mr.ReadingDate)
                .ThenByDescending(mr => mr.PeriodYear)
                .ThenByDescending(mr => mr.PeriodMonth)
                .Select(mr => new MeterReadingDto
                {
                    Id = mr.Id,
                    FlatId = mr.FlatId,
                    FlatNumber = mr.Flat.ApartmentNumber,
                    Type = mr.Type,
                    PeriodYear = mr.PeriodYear,
                    PeriodMonth = mr.PeriodMonth,
                    ReadingValue = mr.ReadingValue,
                    Consumption = mr.Consumption,
                    ReadingDate = mr.ReadingDate,
                    Note = mr.Note,
                    CreatedAt = mr.CreatedAt,
                    UpdatedAt = mr.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<MeterReadingDto>>.Success(meterReadings);
        }
        catch (Exception ex)
        {
            return Result<List<MeterReadingDto>>.Failure($"Sayaç okumaları alınırken hata oluştu: {ex.Message}");
        }
    }
} 