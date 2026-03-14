using AkyildizYonetim.Application.Tenants.Commands.CreateTenant;
using AkyildizYonetim.Application.Tenants.Commands.DeleteTenant;
using AkyildizYonetim.Application.Tenants.Commands.UpdateTenant;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantById;
using AkyildizYonetim.Application.Tenants.Queries.GetTenants;
using AkyildizYonetim.Application.Tenants.Queries.GetTenantStats;
using AkyildizYonetim.Application.Tenants.Queries.GetAvailableFlats;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Policy = "TenantRead")]
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
	private readonly IMediator _mediator;
	public TenantsController(IMediator mediator) { _mediator = mediator; }

	[HttpGet]
	[ProducesResponseType(typeof(PagedResult<TenantDto>), 200)]
	public async Task<IActionResult> GetTenants(
		[FromQuery] bool? isActive,
		[FromQuery] string? searchTerm,
		[FromQuery] bool? showOnlyOccupied,
		[FromQuery(Name = "floor")] int? floorNumber,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		var result = await _mediator.Send(new GetTenantsQuery
		{
			IsActive = isActive,
			SearchTerm = searchTerm,
			ShowOnlyOccupied = showOnlyOccupied,
			FloorNumber = floorNumber,
			PageNumber = pageNumber,
			PageSize = pageSize
		});
		return result.IsSuccess
			? Ok(result.Data)
			: BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	[HttpGet("stats")]
	public async Task<IActionResult> GetTenantStats()
	{
		var result = await _mediator.Send(new GetTenantStatsQuery());
		return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(TenantDto), 200)]
	[ProducesResponseType(typeof(object), 404)]
	public async Task<IActionResult> GetTenantById(Guid id)
	{
		var result = await _mediator.Send(new GetTenantByIdQuery { Id = id });
		return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	// Body doğrudan CreateTenantCommand
	[Authorize(Roles = "admin")]
	[HttpPost]
	[ProducesResponseType(typeof(object), 201)]
	[ProducesResponseType(typeof(object), 400)]
	public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
	{
		if (command is null) return BadRequest(new { error = "Request body is required" });

		var result = await _mediator.Send(command);
		return result.IsSuccess
			? CreatedAtAction(nameof(GetTenantById), new { id = result.Data }, new { id = result.Data, message = "Kiracı başarıyla oluşturuldu" })
			: BadRequest(new { error = "Kiracı oluşturulamadı", message = result.ErrorMessage ?? string.Join(", ", result.Errors) });
	}

	// Body doğrudan UpdateTenantCommand
	[Authorize(Roles = "admin")]
	[HttpPut("{id:guid}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(typeof(object), 400)]
	public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand command)
	{
		if (command is null) return BadRequest(new { error = "Request body is required" });

		// Body'deki Id yoksa route id'yi bas; varsa uyuşmazsa hata ver
		var fixedCommand = command.Id == Guid.Empty ? command with { Id = id } : command;
		if (fixedCommand.Id != id) return BadRequest("Id uyuşmuyor.");

		var result = await _mediator.Send(fixedCommand);
		return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	[Authorize(Roles = "admin")]
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(typeof(object), 404)]
	public async Task<IActionResult> DeleteTenant(Guid id)
	{
		var result = await _mediator.Send(new DeleteTenantCommand { Id = id });
		return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}

	[HttpGet("available-flats")]
	[ProducesResponseType(typeof(List<TenantFlatInfoDto>), 200)]
	[ProducesResponseType(typeof(object), 400)]
	public async Task<IActionResult> GetAvailableFlats(
		[FromQuery(Name = "floor")] int? floorNumber, // geriye dönük: ?floor=
		[FromQuery] string? searchTerm)
	{
		var result = await _mediator.Send(new GetAvailableFlatsQuery { FloorNumber = floorNumber, SearchTerm = searchTerm });
		return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
	}
}
