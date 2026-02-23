using AkyildizYonetim.Application.Flats.Commands.CreateFlat;
using AkyildizYonetim.Application.Flats.Commands.DeleteFlat;
using AkyildizYonetim.Application.Flats.Commands.UpdateFlat;
using AkyildizYonetim.Application.Flats.Queries.GetFlatById;
using AkyildizYonetim.Application.Flats.Queries.GetFlats;
using AkyildizYonetim.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums; // UnitType için

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/[controller]")]
public class FlatsController : ControllerBase
{
	private readonly IMediator _mediator;
	public FlatsController(IMediator mediator) { _mediator = mediator; }

	// ?ownerId=&tenantId=&code=&floorNumber=&type=Floor|Entry|Parking&isOccupied=&isActive=
	[HttpGet]
	public async Task<IActionResult> GetFlats(
		[FromQuery] Guid? ownerId,
		[FromQuery] Guid? tenantId,
		[FromQuery] string? code,
		[FromQuery] int? floorNumber,
		[FromQuery] UnitType? type,
		[FromQuery] bool? isOccupied,
		[FromQuery] bool? isActive)
	{
		var result = await _mediator.Send(new GetFlatsQuery
		{
			OwnerId = ownerId,
			TenantId = tenantId,
			Code = code,
			FloorNumber = floorNumber,
			Type = type,
			IsOccupied = isOccupied,
			IsActive = isActive
		});

		return result.IsSuccess
			? Ok(result.Data)
			: BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetFlatById(Guid id)
	{
		var result = await _mediator.Send(new GetFlatByIdQuery { Id = id });
		return result.IsSuccess
			? Ok(result.Data)
			: NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	// Body: CreateFlatDto
	[HttpPost]
	public async Task<IActionResult> CreateFlat([FromBody] CreateFlatDto dto)
	{
		var result = await _mediator.Send(new CreateFlatCommand { Dto = dto });
		return result.IsSuccess
			? CreatedAtAction(nameof(GetFlatById), new { id = result.Data }, null)
			: BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	// Body: UpdateFlatDto (route id ile DTO.Id eşitlenir)
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> UpdateFlat(Guid id, [FromBody] UpdateFlatDto dto)
	{
		if (id != dto.Id) return BadRequest("Id uyuşmuyor.");

		var result = await _mediator.Send(new UpdateFlatCommand { Dto = dto });
		return result.IsSuccess
			? NoContent()
			: BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteFlat(Guid id)
	{
		var result = await _mediator.Send(new DeleteFlatCommand { Id = id });
		return result.IsSuccess
			? NoContent()
			: NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}
}
