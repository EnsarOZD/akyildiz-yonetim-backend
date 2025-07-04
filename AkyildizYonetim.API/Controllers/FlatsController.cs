using AkyildizYonetim.Application.Flats.Commands.CreateFlat;
using AkyildizYonetim.Application.Flats.Commands.DeleteFlat;
using AkyildizYonetim.Application.Flats.Commands.UpdateFlat;
using AkyildizYonetim.Application.Flats.Queries.GetFlatById;
using AkyildizYonetim.Application.Flats.Queries.GetFlats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
public class FlatsController : ControllerBase
{
    private readonly IMediator _mediator;
    public FlatsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<IActionResult> GetFlats([FromQuery] Guid? ownerId, [FromQuery] Guid? tenantId, [FromQuery] string? number, [FromQuery] int? floor)
    {
        var result = await _mediator.Send(new GetFlatsQuery { OwnerId = ownerId, TenantId = tenantId, Number = number, Floor = floor });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFlatById(Guid id)
    {
        var result = await _mediator.Send(new GetFlatByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost]
    public async Task<IActionResult> CreateFlat([FromBody] CreateFlatCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetFlatById), new { id = result.Data }, null) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFlat(Guid id, [FromBody] UpdateFlatCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id uyuşmuyor.");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFlat(Guid id)
    {
        var result = await _mediator.Send(new DeleteFlatCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }
}

public class CreateFlatRequest
{
    public string FlatNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal SquareMeters { get; set; }
    public int OwnerId { get; set; }
    public string Block { get; set; } = string.Empty;
}

public class UpdateFlatRequest
{
    public string FlatNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal SquareMeters { get; set; }
    public int OwnerId { get; set; }
    public string Block { get; set; } = string.Empty;
} 