using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AkyildizYonetim.Application.Users.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result>
{
    public string OldPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;

    // Resolved from Claims, not from Body
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ResolvedUserId { get; set; }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordPolicyValidator _passwordPolicyValidator;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context, 
        IPasswordHasher passwordHasher,
        IPasswordPolicyValidator passwordPolicyValidator) 
    { 
        _context = context; 
        _passwordHasher = passwordHasher;
        _passwordPolicyValidator = passwordPolicyValidator;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var passwordPolicy = _passwordPolicyValidator.Validate(request.NewPassword);
        if (!passwordPolicy.IsValid)
        {
            return Result.Failure(passwordPolicy.ErrorMessage!);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ResolvedUserId && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Result.Failure("Kullanıcı bulunamadı.");
        
        if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
        {
            if (!_passwordHasher.VerifyLegacyPassword(request.OldPassword, user.PasswordHash))
            {
                return Result.Failure("Mevcut şifre hatalı.");
            }
        }
            
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        // user.RequiresPasswordReset = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}