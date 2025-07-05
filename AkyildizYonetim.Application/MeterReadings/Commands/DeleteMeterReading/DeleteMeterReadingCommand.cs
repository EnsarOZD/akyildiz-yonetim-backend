using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.MeterReadings.Commands.DeleteMeterReading;

public record DeleteMeterReadingCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}

public class DeleteMeterReadingCommandHandler : IRequestHandler<DeleteMeterReadingCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteMeterReadingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteMeterReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var meterReading = await _context.MeterReadings
                .FirstOrDefaultAsync(mr => mr.Id == request.Id && !mr.IsDeleted, cancellationToken);

            if (meterReading == null)
                return Result.Failure("Sayaç okuması bulunamadı");

            // Soft delete
            meterReading.IsDeleted = true;
            meterReading.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Sayaç okuması silinirken hata oluştu: {ex.Message}");
        }
    }
} 