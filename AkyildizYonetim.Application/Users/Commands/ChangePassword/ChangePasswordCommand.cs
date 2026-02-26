using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AkyildizYonetim.Application.Users.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string OldPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public ChangePasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher) 
    { 
        _context = context; 
        _passwordHasher = passwordHasher;
    }
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Result.Failure("Kullanıcı bulunamadı.");
        
        if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
            return Result.Failure("Mevcut şifre hatalı.");
            
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        // user.RequiresPasswordReset = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}