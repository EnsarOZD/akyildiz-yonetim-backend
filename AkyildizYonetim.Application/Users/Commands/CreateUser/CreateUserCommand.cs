using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace AkyildizYonetim.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? TenantId { get; init; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    public CreateUserCommandHandler(IApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Geçici şifre üret
        var tempPassword = GenerateTemporaryPassword(12);
        var passwordHash = HashPassword(tempPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = request.Role,
            OwnerId = request.OwnerId,
            TenantId = request.TenantId,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            // İlk girişte şifre değiştirme zorunlu
            // RequiresPasswordReset = true,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Şifre sıfırlama linki oluştur (örnek token)
        var resetToken = Guid.NewGuid().ToString();
        // Burada gerçek bir token üretimi ve saklanması gerekir (ör: ayrı bir tabloya kaydedilebilir)
        var resetLink = $"https://your-app-url.com/reset-password?email={user.Email}&token={resetToken}";

        // E-posta gönder
        await _emailSender.SendEmailAsync(user.Email, "Hesabınız Oluşturuldu - Şifre Belirleme", $"<p>Sayın {user.FirstName} {user.LastName},</p><p>Hesabınız oluşturuldu. İlk giriş için geçici şifreniz: <b>{tempPassword}</b></p><p>Şifrenizi belirlemek için <a href='{resetLink}'>buraya tıklayın</a>.</p>");

        return Result<Guid>.Success(user.Id);
    }

    private string GenerateTemporaryPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
} 