using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt.CreateAidatForPeriod;
using AkyildizYonetim.Application.UtilityDebts.Commands.UpdateUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Commands.DeleteUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebts;
using AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebtById;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AkyildizYonetim.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UtilityDebtsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;

    public UtilityDebtsController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUtilityDebts([FromQuery] Guid? flatId, [FromQuery] DebtType? type, [FromQuery] int? periodYear, [FromQuery] int? periodMonth, [FromQuery] DebtStatus? status, [FromQuery] Guid? tenantId, [FromQuery] Guid? ownerId)
    {
        var result = await _mediator.Send(new GetUtilityDebtsQuery { FlatId = flatId, Type = type, PeriodYear = periodYear, PeriodMonth = periodMonth, Status = status, TenantId = tenantId, OwnerId = ownerId });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUtilityDebtById(Guid id)
    {
        var result = await _mediator.Send(new GetUtilityDebtByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("last-readings/{type}")]
    public Task<IActionResult> GetLastReadings(string type)
    {
        // Bu endpoint son okumaları getirmek için kullanılacak
        // Şimdilik boş liste döndürüyoruz, daha sonra implement edilecek
        return Task.FromResult<IActionResult>(Ok(new List<object>()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUtilityDebt([FromBody] CreateUtilityDebtCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkUtilityDebts([FromBody] CreateBulkUtilityDebtsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUtilityDebt(Guid id, [FromBody] UpdateUtilityDebtCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUtilityDebt(Guid id)
    {
        var result = await _mediator.Send(new DeleteUtilityDebtCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchUtilityDebt(Guid id, [FromBody] AkyildizYonetim.Application.UtilityDebts.Commands.PatchUtilityDebt.PatchUtilityDebtCommand command)
    {
        var fixedCommand = command with { Id = id };
        var result = await _mediator.Send(fixedCommand);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("period/{period}")]
    public Task<IActionResult> DeleteByPeriod(string period)
    {
        // Bu endpoint dönem bazında silme için kullanılacak
        // Şimdilik başarılı döndürüyoruz, daha sonra implement edilecek
        return Task.FromResult<IActionResult>(NoContent());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("distribute")]
    public async Task<IActionResult> DistributeSharedExpense([FromBody] DistributeSharedExpenseRequest request)
    {
        try
        {
            // Dönem bilgisini parse et
            if (!DateTime.TryParseExact(request.Period, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime periodDate))
            {
                return BadRequest("Geçersiz dönem formatı. YYYY-MM formatında olmalı.");
            }

            // Utility type'ı enum'a çevir
            if (!Enum.TryParse<DebtType>(request.UtilityType, out DebtType utilityType))
            {
                return BadRequest("Geçersiz gider tipi.");
            }

            // Ortak gider kaydını bul
            var sharedExpense = await _context.UtilityDebts
                .Where(d => d.PeriodYear == periodDate.Year && 
                           d.PeriodMonth == periodDate.Month && 
                           d.Type == utilityType &&
                           (d.Description == "Ortak Alan" || d.Description == "Mescit"))
                .FirstOrDefaultAsync();

            if (sharedExpense == null)
            {
                return BadRequest("Bu dönem için ortak gider kaydı bulunamadı.");
            }

            // Aktif (dolu) üniteleri getir
            var activeFlats = await _context.Flats
                .Where(f => !f.IsDeleted && f.TenantId != null && f.IsOccupied)
                .Include(f => f.Tenant)
                .ToListAsync();

            if (!activeFlats.Any())
            {
                return BadRequest("Dolu ünite (aktif kiracı) bulunamadı.");
            }

            // Ünite başına düşen tutarı hesapla
            decimal amountPerFlat = sharedExpense.Amount / activeFlats.Count;

            // Her ünite için borç kaydı oluştur
            var createdDebtsCount = 0;
            foreach (var flat in activeFlats)
            {
                var defaultDueDate = new DateTime(periodDate.Year, periodDate.Month, 1).AddDays(9);

                var debt = new UtilityDebt
                {
                    Id = Guid.NewGuid(),
                    FlatId = flat.Id,
                    Type = utilityType,
                    PeriodYear = periodDate.Year,
                    PeriodMonth = periodDate.Month,
                    Amount = amountPerFlat,
                    RemainingAmount = amountPerFlat,
                    Status = DebtStatus.Unpaid,
                    DueDate = defaultDueDate,
                    Description = $"Ortak {request.UtilityType} Payı - {flat.Code} ({flat.Tenant?.CompanyName})",
                    TenantId = flat.TenantId,
                    OwnerId = flat.OwnerId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UtilityDebts.Add(debt);
                createdDebtsCount++;
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Ortak gider başarıyla paylaştırıldı", 
                sharedExpenseAmount = sharedExpense.Amount,
                unitCount = activeFlats.Count,
                amountPerFlat = amountPerFlat,
                createdDebtsCount = createdDebtsCount
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Ortak gider paylaştırılırken hata oluştu: {ex.Message}");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("create-aidat")]
    public async Task<IActionResult> CreateAidat([FromBody] CreateAidatRequest request, CancellationToken ct)
    {
        var (tenantDuesCreated, ownerDuesCreated) =
            await _mediator.Send(new CreateAidatForPeriodCommand(request.Period, request.DueDate), ct);

        return Ok(new { tenantDuesCreated, ownerDuesCreated });
    }
}

public class DistributeSharedExpenseRequest
{
    public string Period { get; set; } = string.Empty;
    public string UtilityType { get; set; } = string.Empty;
}

public sealed class CreateAidatRequest
{
    // "YYYY-MM" (ör. 2025-03)
    public string Period { get; set; } = default!;
    // Son ödeme tarihi (zorunlu)
    public DateTime DueDate { get; set; }
}