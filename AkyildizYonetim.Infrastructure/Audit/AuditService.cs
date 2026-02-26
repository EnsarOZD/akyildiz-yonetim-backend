using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AkyildizYonetim.Infrastructure.Audit;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        AuditAction action,
        AuditEntityType entityType,
        Guid entityId,
        string? entityName = null,
        string? description = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = GetCurrentUserId(httpContext);
            var userName = GetCurrentUserName(httpContext);
            
            ipAddress ??= GetClientIpAddress(httpContext);
            userAgent ??= GetUserAgent(httpContext);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                UserId = userId,
                UserName = userName,
                Description = description,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit log kaydedildi: {Action} {EntityType} {EntityId}", 
                action, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit log kaydedilirken hata oluştu: {Action} {EntityType} {EntityId}", 
                action, entityType, entityId);
        }
    }

    public async Task LogPaymentAsync(
        Guid paymentId,
        decimal amount,
        PaymentType type,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            AuditAction.Payment,
            AuditEntityType.Payment,
            paymentId,
            $"Ödeme - {type}",
            description ?? $"Ödeme oluşturuldu: {amount:C}",
            null,
            new { Amount = amount, Type = type },
            ipAddress,
            userAgent,
            cancellationToken);
    }

    public async Task LogDebtAllocationAsync(
        Guid paymentId,
        Guid debtId,
        decimal allocatedAmount,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            AuditAction.DebtAllocation,
            AuditEntityType.PaymentDebt,
            paymentId,
            "Borç Eşleştirme",
            description ?? $"Borç eşleştirildi: {allocatedAmount:C}",
            null,
            new { PaymentId = paymentId, DebtId = debtId, AllocatedAmount = allocatedAmount },
            ipAddress,
            userAgent,
            cancellationToken);
    }

    private static string GetCurrentUserId(HttpContext? httpContext)
    {
        // JWT token'dan user ID'yi al
        var userId = httpContext?.User?.FindFirst("sub")?.Value 
                  ?? httpContext?.User?.FindFirst("nameid")?.Value
                  ?? "system";
        return userId;
    }

    private static string? GetCurrentUserName(HttpContext? httpContext)
    {
        return httpContext?.User?.FindFirst("name")?.Value;
    }

    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        return httpContext?.Connection?.RemoteIpAddress?.ToString()
            ?? httpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? httpContext?.Request?.Headers["X-Real-IP"].FirstOrDefault();
    }

    private static string? GetUserAgent(HttpContext? httpContext)
    {
        return httpContext?.Request?.Headers["User-Agent"].FirstOrDefault();
    }
} 