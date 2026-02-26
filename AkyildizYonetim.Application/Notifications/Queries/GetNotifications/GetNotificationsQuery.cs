using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery : IRequest<NotificationListDto>
{
    public Guid UserId { get; init; }
    public bool UnreadOnly { get; init; } = false;
    public int Take { get; init; } = 20;
    public int Skip { get; init; } = 0;
}

public class NotificationListDto
{
    public List<Notification> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
}

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, NotificationListDto>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationListDto> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take > 50 ? 50 : request.Take;

        var query = _context.Notifications
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(request.Skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new NotificationListDto
        {
            Items = items,
            TotalCount = totalCount,
            UnreadCount = unreadCount
        };
    }
}
