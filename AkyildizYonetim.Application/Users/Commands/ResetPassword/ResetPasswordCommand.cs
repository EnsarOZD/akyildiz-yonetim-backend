using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AkyildizYonetim.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ResetPasswordCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Email == request.Email && 
            u.PasswordResetToken == request.Token &&
            !u.IsDeleted, cancellationToken);

        if (user == null)
            return Result.Failure("Geçersiz e-posta veya token.");

        if (user.ResetTokenExpires < DateTime.UtcNow)
            return Result.Failure("Şifre sıfırlama bağlantısının süresi dolmuş.");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
} 