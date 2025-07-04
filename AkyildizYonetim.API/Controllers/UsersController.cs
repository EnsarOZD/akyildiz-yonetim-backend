using AkyildizYonetim.Application.Users.Commands.CreateUser;
using AkyildizYonetim.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    
    public UsersController(IMediator mediator, IApplicationDbContext context) 
    { 
        _mediator = mediator; 
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("debug")]
    public async Task<IActionResult> GetUsersDebug()
    {
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync();
            
        return Ok(new
        {
            TotalUsers = users.Count,
            Users = users
        });
    }
} 