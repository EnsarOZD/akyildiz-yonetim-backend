using AkyildizYonetim.Application.Users.Commands.CreateUser;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using AkyildizYonetim.Application.Common.Models;

using Microsoft.AspNetCore.Authorization;

namespace AkyildizYonetim.API.Controllers;

[Authorize(Roles = "admin,manager")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ClientSettings _clientSettings;
    
    public UsersController(
        IMediator mediator, 
        IApplicationDbContext context, 
        IEmailSender emailSender,
        IOptions<ClientSettings> clientSettings) 
    { 
        _mediator = mediator; 
        _context = context;
        _emailSender = emailSender;
        _clientSettings = clientSettings.Value;
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // ... (existing code remains as is)
        var role = MapStringToRole(request.Role);
        
        var command = new CreateUserCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = role,
            TenantId = role == UserRole.Tenant ? request.CompanyId : null,
            OwnerId  = role == UserRole.Owner  ? request.CompanyId : null,
        };
        
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage ?? string.Join(", ", result.Errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // ... (existing code remains as is)
        var usersData = await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.IsActive,
                CompanyName = u.Tenant != null ? u.Tenant.CompanyName : (u.Owner != null ? u.Owner.FirstName + " " + u.Owner.LastName : null),
                u.CreatedAt
            })
            .ToListAsync();

        var users = usersData.Select(u => new
        {
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            Role = MapRoleToString(u.Role),
            u.IsActive,
            u.CompanyName,
            u.CreatedAt
        });

        return Ok(users);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        // ... (existing code remains as is)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Email != null) user.Email = request.Email;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (request.Role != null)
        {
            var newRole = MapStringToRole(request.Role);
            user.Role = newRole;
            if (request.CompanyId.HasValue)
            {
                user.TenantId = newRole == UserRole.Tenant ? request.CompanyId : null;
                user.OwnerId  = newRole == UserRole.Owner  ? request.CompanyId : null;
            }
        }

        await _context.SaveChangesAsync(default);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromServices] IAppUrlBuilder urlBuilder)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        // Şifre sıfırlama token'ı oluştur
        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(default);

        // Şifre sıfırlama linki
        var resetLink = urlBuilder.BuildResetPasswordLink(resetToken, user.Email);

        // E-posta gÃ¶nder
        await _emailSender.SendPasswordResetEmailAsync(user.Email, resetLink);

        return Ok(new { message = "Åifre sÄ±fÄ±rlama baÄŸlantÄ±sÄ± e-posta ile gÃ¶nderildi." });
    }

    [Authorize(Roles = "admin")]
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
                Role = u.Role.ToString(),
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

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = new[]
        {
            new { code = "admin", label = "Sistem Yöneticisi", requiresTenant = false },
            new { code = "manager", label = "Yönetici (Müdür)", requiresTenant = false },
            new { code = "owner", label = "Mal Sahibi", requiresTenant = false },
            new { code = "tenant", label = "Kiracı", requiresTenant = true },
            new { code = "observer", label = "Gözlemci (Avukat vb.)", requiresTenant = false },
            new { code = "dataentry", label = "Veri Giriş Sorumlusu", requiresTenant = false }
        };
        return Ok(roles);
    }

    private static UserRole MapStringToRole(string role)
    {
        if (string.IsNullOrEmpty(role)) return UserRole.Observer;
        
        return role.ToLower() switch
        {
            "admin" => UserRole.Admin,
            "manager" => UserRole.Manager,
            "owner" => UserRole.Owner,
            "tenant" => UserRole.Tenant,
            "observer" => UserRole.Observer,
            "dataentry" => UserRole.DataEntry,
            _ => UserRole.Observer
        };
    }

    private static string MapRoleToString(UserRole role)
    {
        return role.ToString().ToLower();
    }

    private string GenerateTemporaryPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
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

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? CompanyId { get; set; }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public Guid? CompanyId { get; set; }
}