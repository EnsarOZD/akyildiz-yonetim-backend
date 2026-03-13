using AkyildizYonetim.Application.Users.Commands.Login;
using AkyildizYonetim.Application.Users.Commands.ResetPassword;
using AkyildizYonetim.Application.Users.Commands.ChangePassword;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using AkyildizYonetim.Application.Common.Interfaces;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly ILoginAttemptService _loginAttemptService;
    
    public AuthController(IMediator mediator, ILogger<AuthController> logger, ILoginAttemptService loginAttemptService) 
    { 
        _mediator = mediator; 
        _logger = logger;
        _loginAttemptService = loginAttemptService;
    }
 
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        if (await _loginAttemptService.IsLockedOutAsync(command.Email))
        {
            _logger.LogWarning("Login blocked due to lockout: {Email}", command.Email);
            return StatusCode(StatusCodes.Status423Locked, "Çok fazla hatalı deneme. Lütfen daha sonra tekrar deneyiniz.");
        }

        _logger.LogInformation("Login attempt for email: {Email}", command.Email);
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Login successful for user: {Email}", command.Email);
            await _loginAttemptService.ResetAttemptsAsync(command.Email);
            return Ok(result.Data);
        }
        else
        {
            _logger.LogWarning("Login failed for user: {Email}. Error: {Error}", command.Email, result.ErrorMessage ?? string.Join(", ", result.Errors));
            await _loginAttemptService.RegisterFailedAttemptAsync(command.Email);
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
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        command.ResolvedUserId = userId;
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

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok();
    }
}