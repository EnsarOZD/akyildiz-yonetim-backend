using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<Result<List<AuditLogDto>>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public AuditEntityType? EntityType { get; init; }
    public string? UserId { get; init; }
    public AuditAction? Action { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<List<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            // Filtreler
            if (request.StartDate.HasValue)
                query = query.Where(a => a.Timestamp >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.Timestamp <= request.EndDate.Value);

            if (request.EntityType.HasValue)
                query = query.Where(a => a.EntityType == request.EntityType.Value);

            if (!string.IsNullOrEmpty(request.UserId))
                query = query.Where(a => a.UserId == request.UserId);

            if (request.Action.HasValue)
                query = query.Where(a => a.Action == request.Action.Value);

            // Sıralama ve sayfalama
            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    EntityType = a.EntityType.ToString(),
                    EntityId = a.EntityId,
                    Action = a.Action.ToString(),
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    UserId = a.UserId,
                    UserName = a.UserName ?? "Sistem",
                    UserEmail = "", // AuditLog'da UserEmail yok
                    IpAddress = a.IpAddress ?? "",
                    UserAgent = a.UserAgent ?? "",
                    Timestamp = a.Timestamp,
                    Description = a.Description ?? ""
                })
                .ToListAsync(cancellationToken);

            return Result<List<AuditLogDto>>.Success(auditLogs);
        }
        catch (Exception ex)
        {
            return Result<List<AuditLogDto>>.Failure($"Audit logları alınırken hata oluştu: {ex.Message}");
        }
    }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
} 