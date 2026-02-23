using AkyildizYonetim.Application.AdvanceAccounts.Commands.CreateAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Commands.DeleteAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Commands.UpdateAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccountById;
using AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/[controller]")]
public class AdvanceAccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdvanceAccountsController> _logger;

    public AdvanceAccountsController(IMediator mediator, ILogger<AdvanceAccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdvanceAccounts(
        [FromQuery] Guid? tenantId,
        [FromQuery] bool? activeOnly)
    {
        try
        {
            _logger.LogInformation("Avans hesapları getiriliyor: TenantId={TenantId}, ActiveOnly={ActiveOnly}", 
                tenantId, activeOnly);

            var result = await _mediator.Send(new GetAdvanceAccountsQuery
            {
                TenantId = tenantId,
                ActiveOnly = activeOnly ?? true
            });

            if (result.IsSuccess)
            {
                _logger.LogInformation("Avans hesapları başarıyla getirildi: Count={Count}", result.Data?.Count ?? 0);
                return Ok(result.Data);
            }

            _logger.LogWarning("Avans hesapları getirilemedi: {Error}", result.ErrorMessage ?? string.Join(", ", result.Errors));
            return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans hesapları getirilirken beklenmeyen hata oluştu");
            return StatusCode(500, "Avans hesapları getirilirken bir hata oluştu.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdvanceAccountById(Guid id)
    {
        try
        {
            _logger.LogInformation("Avans hesabı getiriliyor: Id={Id}", id);

            var result = await _mediator.Send(new GetAdvanceAccountByIdQuery { Id = id });
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Avans hesabı başarıyla getirildi: Id={Id}", id);
                return Ok(result.Data);
            }

            _logger.LogWarning("Avans hesabı bulunamadı: Id={Id}, Error={Error}", 
                id, result.ErrorMessage ?? string.Join(", ", result.Errors));
            return NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans hesabı getirilirken beklenmeyen hata oluştu: Id={Id}", id);
            return StatusCode(500, "Avans hesabı getirilirken bir hata oluştu.");
        }
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost]
    public async Task<IActionResult> CreateAdvanceAccount([FromBody] CreateAdvanceAccountRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Geçersiz model durumu: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Avans hesabı oluşturuluyor: TenantId={TenantId}, Balance={Balance}", 
                request.TenantId, request.Balance);

            var command = new CreateAdvanceAccountCommand
            {
                TenantId = request.TenantId,
                Balance = request.Balance,
                Description = request.Description
            };

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Avans hesabı başarıyla oluşturuldu: Id={Id}", result.Data);
                return CreatedAtAction(nameof(GetAdvanceAccountById), new { id = result.Data }, null);
            }

            _logger.LogWarning("Avans hesabı oluşturulamadı: {Error}", 
                result.ErrorMessage ?? string.Join(", ", result.Errors));
            return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans hesabı oluşturulurken beklenmeyen hata oluştu");
            return StatusCode(500, "Avans hesabı oluşturulurken bir hata oluştu.");
        }
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdvanceAccount(Guid id, [FromBody] UpdateAdvanceAccountRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Geçersiz model durumu: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Avans hesabı güncelleniyor: Id={Id}, TenantId={TenantId}, Balance={Balance}", 
                id, request.TenantId, request.Balance);

            var command = new UpdateAdvanceAccountCommand
            {
                Id = id,
                TenantId = request.TenantId,
                Balance = request.Balance,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Avans hesabı başarıyla güncellendi: Id={Id}", id);
                return NoContent();
            }

            _logger.LogWarning("Avans hesabı güncellenemedi: Id={Id}, Error={Error}", 
                id, result.ErrorMessage ?? string.Join(", ", result.Errors));
            return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans hesabı güncellenirken beklenmeyen hata oluştu: Id={Id}", id);
            return StatusCode(500, "Avans hesabı güncellenirken bir hata oluştu.");
        }
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdvanceAccount(Guid id)
    {
        try
        {
            _logger.LogInformation("Avans hesabı siliniyor: Id={Id}", id);

            var result = await _mediator.Send(new DeleteAdvanceAccountCommand { Id = id });
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Avans hesabı başarıyla silindi: Id={Id}", id);
                return NoContent();
            }

            _logger.LogWarning("Avans hesabı silinemedi: Id={Id}, Error={Error}", 
                id, result.ErrorMessage ?? string.Join(", ", result.Errors));
            return NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans hesabı silinirken beklenmeyen hata oluştu: Id={Id}", id);
            return StatusCode(500, "Avans hesabı silinirken bir hata oluştu.");
        }
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost("use")]
    public async Task<IActionResult> UseAdvanceAccount([FromBody] UseAdvanceAccountRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Geçersiz model durumu: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Avans hesabı kullanılıyor: TenantId={TenantId}, DebtCount={DebtCount}", 
                request.TenantId, request.DebtPayments?.Count ?? 0);

            var command = new UseAdvanceAccountCommand
            {
                TenantId = request.TenantId,
                DebtPayments = request.DebtPayments ?? new(),
                Description = request.Description
            };

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Avans hesabı başarıyla kullanıldı: TenantId={TenantId}, Amount={Amount}", 
                    request.TenantId, result.Data?.TotalAmount ?? 0);
                return Ok(result.Data);
            }

            _logger.LogWarning("Avans hesabı kullanılamadı: TenantId={TenantId}, Error={Error}", 
                request.TenantId, result.ErrorMessage ?? string.Join(", ", result.Errors));
            return BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans hesabı kullanılırken beklenmeyen hata oluştu: TenantId={TenantId}", request.TenantId);
            return StatusCode(500, "Avans hesabı kullanılırken bir hata oluştu.");
        }
    }
} 