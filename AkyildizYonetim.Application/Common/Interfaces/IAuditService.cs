using AkyildizYonetim.Domain.Entities;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        AuditAction action,
        AuditEntityType entityType,
        Guid entityId,
        string? entityName = null,
        string? description = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
    
    Task LogPaymentAsync(
        Guid paymentId,
        decimal amount,
        PaymentType type,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
    
    Task LogDebtAllocationAsync(
        Guid paymentId,
        Guid debtId,
        decimal allocatedAmount,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
} 