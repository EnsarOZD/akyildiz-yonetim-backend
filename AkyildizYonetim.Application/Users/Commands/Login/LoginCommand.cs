using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public LoginCommandHandler(IApplicationDbContext context) { _context = context; }
    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Result<LoginResultDto>.Failure("Kullanıcı bulunamadı.");
        if (user.PasswordHash != HashPassword(request.Password))
            return Result<LoginResultDto>.Failure("Şifre hatalı.");
        if (!user.IsActive)
            return Result<LoginResultDto>.Failure("Kullanıcı pasif durumda.");
        var result = new LoginResultDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            RequiresPasswordReset = false // user.RequiresPasswordReset
        };
        return Result<LoginResultDto>.Success(result);
    }
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public class LoginResultDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool RequiresPasswordReset { get; set; }
} 