using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AkyildizYonetim.Application.Users.Commands.Login;

public record LoginCommand : IRequest<Result<LoginResultDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    
    public LoginCommandHandler(IApplicationDbContext context, ILogger<LoginCommandHandler> logger, IJwtService jwtService, IPasswordHasher passwordHasher) 
    { 
        _context = context; 
        _logger = logger;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching for user with email: {Email}", request.Email);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", request.Email);
            return Result<LoginResultDto>.Failure("Kullanıcı bulunamadı.");
        }
        
        _logger.LogInformation("User found: {Email}, IsActive: {IsActive}", user.Email, user.IsActive);
        
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Fallback for legacy SHA-256 hashes
            if (_passwordHasher.VerifyLegacyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogInformation("Legacy password verified for user: {Email}. Upgrading to BCrypt.", request.Email);
                
                // Upgrade hash to BCrypt
                user.PasswordHash = _passwordHasher.HashPassword(request.Password);
                user.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("Password mismatch for user: {Email}", request.Email);
                return Result<LoginResultDto>.Failure("Şifre hatalı.");
            }
        }
        
        if (!user.IsActive)
        {
            _logger.LogWarning("Inactive user login attempt: {Email}", request.Email);
            return Result<LoginResultDto>.Failure("Kullanıcı pasif durumda.");
        }
        
        _logger.LogInformation("Login successful for user: {Email}, Role: {Role}", user.Email, user.Role);
        
        // JWT token üret
        var token = _jwtService.GenerateToken(user);
        
        var result = new LoginResultDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            RequiresPasswordReset = false // user.RequiresPasswordReset
        };
        return Result<LoginResultDto>.Success(result);
    }
}

public class LoginResultDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool RequiresPasswordReset { get; set; }
} 