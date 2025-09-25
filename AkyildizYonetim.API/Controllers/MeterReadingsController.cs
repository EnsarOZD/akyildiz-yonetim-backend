using AkyildizYonetim.Application.MeterReadings.Commands;
using AkyildizYonetim.Application.MeterReadings.Commands.CreateMeterReading;
using AkyildizYonetim.Application.MeterReadings.Commands.UpdateMeterReading;
using AkyildizYonetim.Application.MeterReadings.Commands.DeleteMeterReading;
using AkyildizYonetim.Application.MeterReadings.Commands.BulkUpsertMeterReadings;
using AkyildizYonetim.Application.MeterReadings.Commands.ApplySharedConsumption;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.MeterReadings.Queries.GetMeterReadings;
using AkyildizYonetim.Application.MeterReadings.Queries.GetMeterReadingById;
using AkyildizYonetim.Application.MeterReadings.DTOs;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MeterReadingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUtilityConfigurationService _utilityConfigService;

    public MeterReadingsController(IMediator mediator, IUtilityConfigurationService utilityConfigService)
    {
        _mediator = mediator;
        _utilityConfigService = utilityConfigService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMeterReadings(
        [FromQuery] Guid? flatId, 
        [FromQuery] MeterType? type, 
        [FromQuery] int? periodYear, 
        [FromQuery] int? periodMonth,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _mediator.Send(new GetMeterReadingsQuery 
        { 
            FlatId = flatId, 
            Type = type, 
            PeriodYear = periodYear, 
            PeriodMonth = periodMonth,
            StartDate = startDate,
            EndDate = endDate
        });
        
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeterReadingById(Guid id)
    {
        var result = await _mediator.Send(new GetMeterReadingByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeterReading([FromBody] CreateMeterReadingCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { Id = result.Data }) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeterReading(Guid id, [FromBody] UpdateMeterReadingCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID uyuşmazlığı");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeterReading(Guid id)
    {
        var result = await _mediator.Send(new DeleteMeterReadingCommand { Id = id });
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("last-readings/{flatId}/{type}")]
    public async Task<IActionResult> GetLastReading(Guid flatId, MeterType type)
    {
        var result = await _mediator.Send(new GetMeterReadingsQuery 
        { 
            FlatId = flatId, 
            Type = type 
        });
        
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));

        var lastReading = result.Data?.OrderByDescending(r => r.ReadingDate).FirstOrDefault();
        return Ok(lastReading);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetMeterReadingStats(
        [FromQuery] int? year, 
        [FromQuery] int? month, 
        [FromQuery] MeterType? type)
    {
        var result = await _mediator.Send(new GetMeterReadingsQuery 
        { 
            PeriodYear = year, 
            PeriodMonth = month, 
            Type = type 
        });
        
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));

        var readings = result.Data ?? new List<MeterReadingDto>();
        
        var stats = new
        {
            TotalReadings = readings.Count,
            TotalConsumption = readings.Sum(r => r.Consumption),
            AverageConsumption = readings.Any() ? readings.Average(r => r.Consumption) : 0,
            ByType = readings.GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count(), TotalConsumption = g.Sum(r => r.Consumption) })
                .ToList()
        };

        return Ok(stats);
    }

    [HttpPost("distribute-shared-consumption")]
    public async Task<IActionResult> DistributeSharedConsumption([FromBody] DistributeSharedConsumptionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("apply-shared-consumption")]
    public async Task<IActionResult> ApplySharedConsumption([FromBody] ApplySharedConsumptionCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("pricing/{year}/{month}/{type}")]
    public async Task<IActionResult> GetPricing(int year, int month, MeterType type)
    {
        try
        {
            var pricing = await _utilityConfigService.GetPricingAsync(year, month, type);
            return Ok(pricing);
        }
        catch (Exception ex)
        {
            return BadRequest($"Fiyatlandırma bilgileri alınırken hata oluştu: {ex.Message}");
        }
    }

    [HttpPost("bulk-upsert")]
public async Task<IActionResult> BulkUpsert([FromBody] BulkUpsertMeterReadingsCommand command)
{
    var result = await _mediator.Send(command);
    return result.IsSuccess 
        ? Ok(new { affected = result.Data }) 
        : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
}
} 