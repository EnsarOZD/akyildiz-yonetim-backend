using AkyildizYonetim.Application.AidatDefinitions.Commands.CreateAidatDefinition;
using AkyildizYonetim.Application.AidatDefinitions.Commands.DeleteAidatDefinition;
using AkyildizYonetim.Application.AidatDefinitions.Commands.UpdateAidatDefinition;
using AkyildizYonetim.Application.AidatDefinitions.Queries.GetAidatDefinitionById;
using AkyildizYonetim.Application.AidatDefinitions.Queries.GetAidatDefinitions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AkyildizYonetim.Application.Common.Models;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/aidat-definitions")]
public class AidatDefinitionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AidatDefinitionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AkyildizYonetim.Application.AidatDefinitions.Queries.GetAidatDefinitions.AidatDefinitionDto>), 200)]
    public async Task<IActionResult> GetAidatDefinitions(
        [FromQuery] Guid? tenantId,
        [FromQuery] int? year,
        [FromQuery] string? unit,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAidatDefinitionsQuery
        {
            TenantId = tenantId,
            Year = year,
            Unit = unit,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
 
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAidatDefinitionById(Guid id)
    {
        var result = await _mediator.Send(new GetAidatDefinitionByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPost]
    public async Task<IActionResult> CreateAidatDefinition([FromBody] CreateAidatDefinitionCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetAidatDefinitionById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAidatDefinition(Guid id, [FromBody] UpdateAidatDefinitionCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [Authorize(Policy = "TenantWrite")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAidatDefinition(Guid id)
    {
        var result = await _mediator.Send(new DeleteAidatDefinitionCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
} 