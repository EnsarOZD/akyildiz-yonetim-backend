using AkyildizYonetim.Application.Users.Commands.Login;
using AkyildizYonetim.Application.Users.Commands.ResetPassword;
using AkyildizYonetim.Application.Users.Commands.ChangePassword;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IMediator mediator, ILogger<AuthController> logger) 
    { 
        _mediator = mediator; 
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Login attempt for email: {Email}", command.Email);
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Login successful for user: {Email}", command.Email);
            return Ok(result.Data);
        }
        else
        {
            _logger.LogWarning("Login failed for user: {Email}. Error: {Error}", command.Email, result.ErrorMessage ?? string.Join(", ", result.Errors));
            return Unauthorized(result.ErrorMessage ?? string.Join(", ", result.Errors));
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new
        {
            UserId = userId,
            Email = email,
            Name = name,
            Role = role
        });
    }
} 