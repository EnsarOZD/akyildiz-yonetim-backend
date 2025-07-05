using AkyildizYonetim.Application.AdvanceAccounts.Commands.CreateAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Commands.DeleteAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Commands.UpdateAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;
using AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccountById;
using AkyildizYonetim.Application.AdvanceAccounts.Queries.GetAdvanceAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AdvanceAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdvanceAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdvanceAccounts(
        [FromQuery] Guid? tenantId,
        [FromQuery] bool? activeOnly)
    {
        var result = await _mediator.Send(new GetAdvanceAccountsQuery
        {
            TenantId = tenantId,
            ActiveOnly = activeOnly ?? true
        });

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdvanceAccountById(Guid id)
    {
        var result = await _mediator.Send(new GetAdvanceAccountByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdvanceAccount([FromBody] CreateAdvanceAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetAdvanceAccountById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdvanceAccount(Guid id, [FromBody] UpdateAdvanceAccountCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdvanceAccount(Guid id)
    {
        var result = await _mediator.Send(new DeleteAdvanceAccountCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost("use")]
    public async Task<IActionResult> UseAdvanceAccount([FromBody] UseAdvanceAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
} 