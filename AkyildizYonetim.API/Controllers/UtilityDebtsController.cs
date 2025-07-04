using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Commands.UpdateUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Commands.DeleteUtilityDebt;
using AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebts;
using AkyildizYonetim.Application.UtilityDebts.Queries.GetUtilityDebtById;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
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

    [HttpPost]
    public async Task<IActionResult> CreateUtilityDebt([FromBody] CreateUtilityDebtCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUtilityDebt(Guid id, [FromBody] UpdateUtilityDebtCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUtilityDebt(Guid id)
    {
        var result = await _mediator.Send(new DeleteUtilityDebtCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("period/{period}")]
    public Task<IActionResult> DeleteByPeriod(string period)
    {
        // Bu endpoint dönem bazında silme için kullanılacak
        // Şimdilik başarılı döndürüyoruz, daha sonra implement edilecek
        return Task.FromResult<IActionResult>(NoContent());
    }

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

            // Aktif kiracıları getir
            var activeTenants = await _context.Tenants
                .Where(t => t.IsActive && !t.IsDeleted)
                .ToListAsync();

            if (!activeTenants.Any())
            {
                return BadRequest("Aktif kiracı bulunamadı.");
            }

            // Kiracı başına düşen tutarı hesapla
            decimal amountPerTenant = sharedExpense.Amount / activeTenants.Count;

            // Her kiracı için borç kaydı oluştur
            var createdDebts = new List<Guid>();
            foreach (var tenant in activeTenants)
            {
                // Kiracının daire bilgisini bul
                var flat = await _context.Flats
                    .Where(f => f.TenantId == tenant.Id && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (flat != null)
                {
                    var debt = new UtilityDebt
                    {
                        Id = Guid.NewGuid(),
                        FlatId = flat.Id,
                        Type = utilityType,
                        PeriodYear = periodDate.Year,
                        PeriodMonth = periodDate.Month,
                        Amount = amountPerTenant,
                        Status = DebtStatus.Unpaid,
                        Description = $"Ortak {request.UtilityType} Payı - {tenant.FirstName} {tenant.LastName}",
                        TenantId = tenant.Id,
                        OwnerId = flat.OwnerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UtilityDebts.Add(debt);
                    createdDebts.Add(debt.Id);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Ortak gider başarıyla paylaştırıldı", 
                sharedExpenseAmount = sharedExpense.Amount,
                tenantCount = activeTenants.Count,
                amountPerTenant = amountPerTenant,
                createdDebtsCount = createdDebts.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Ortak gider paylaştırılırken hata oluştu: {ex.Message}");
        }
    }

    [HttpPost("create-aidat")]
    public Task<IActionResult> CreateAidat([FromBody] CreateAidatRequest request)
    {
        // Bu endpoint aidat oluşturma için kullanılacak
        // Şimdilik başarılı döndürüyoruz, daha sonra implement edilecek
        return Task.FromResult<IActionResult>(Ok(new { tenantDuesCreated = 0, ownerDuesCreated = 0 }));
    }
}

public class DistributeSharedExpenseRequest
{
    public string Period { get; set; } = string.Empty;
    public string UtilityType { get; set; } = string.Empty;
}

public class CreateAidatRequest
{
    public string Period { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public int Year { get; set; }
} 