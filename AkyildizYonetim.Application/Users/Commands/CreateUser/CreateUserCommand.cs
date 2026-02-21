using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;

using Microsoft.Extensions.Options;
using AkyildizYonetim.Application.Common.Models;

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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateUserCommandHandler(
        IApplicationDbContext context, 
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ClientSettings> clientSettings)
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
        _clientSettings = clientSettings.Value;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // E-posta benzersizliği kontrolü
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (existingUser)
        {
            return Result<Guid>.Failure("Bu e-posta adresi zaten kullanımda.");
        }

        // Şifre sıfırlama token'ı oluştur
        var resetToken = Guid.NewGuid().ToString("N");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = request.Role,
            OwnerId = request.OwnerId,
            TenantId = request.TenantId,
            PasswordHash = string.Empty, // Şifre henüz yok
            PasswordResetToken = resetToken,
            ResetTokenExpires = DateTime.UtcNow.AddDays(3),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Şifre belirleme linki (Frontend URL)
        var resetLink = $"{_clientSettings.ClientUrl}/set-password?token={resetToken}&email={user.Email}";
        var mailBody = $"<p>Sayın {user.FirstName} {user.LastName},</p>" +
                       $"<p>Akyıldız Yönetim sisteminde hesabınız başarıyla oluşturuldu.</p>" +
                       $"<p>Giriş yapabilmek için lütfen aşağıdaki bağlantıya tıklayarak şifrenizi belirleyin:</p>" +
                       $"<p><a href='{resetLink}' style='padding: 10px 20px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 5px;'>Şifremi Belirle</a></p>" +
                       $"<p>Bu bağlantı 3 gün boyunca geçerlidir.</p>" +
                       $"<p>Eğer butona tıklayamıyorsanız şu bağlantıyı tarayıcınıza yapıştırabilirsiniz:<br>{resetLink}</p>";

        // E-posta gönderimini arka plana at (UI beklemesin)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await emailSender.SendEmailAsync(request.Email, "Hesabınız Oluşturuldu - Şifre Belirleme", mailBody);
            }
            catch (Exception ex)
            {
                // Hata günlüğe kaydedilir (System.Console veya ILogger)
                System.Console.WriteLine($"❌ Arka planda e-posta gönderim hatası: {ex.Message}");
            }
        });

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