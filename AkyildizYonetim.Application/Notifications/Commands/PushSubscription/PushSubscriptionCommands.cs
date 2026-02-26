using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Notifications.Commands.PushSubscription;

public record SubscribePushCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string P256dh { get; init; } = string.Empty;
    public string Auth { get; init; } = string.Empty;
}

public record UnsubscribePushCommand : IRequest<Result>
{
    public string Endpoint { get; init; } = string.Empty;
}

public class PushSubscriptionCommandHandler : 
    IRequestHandler<SubscribePushCommand, Result>,
    IRequestHandler<UnsubscribePushCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public PushSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SubscribePushCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.UserPushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == request.Endpoint, cancellationToken);

        if (existing != null)
        {
            existing.P256dh = request.P256dh;
            existing.Auth = request.Auth;
            existing.UserId = request.UserId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var subscription = new UserPushSubscription
            {
                UserId = request.UserId,
                Endpoint = request.Endpoint,
                P256dh = request.P256dh,
                Auth = request.Auth,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserPushSubscriptions.Add(subscription);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(UnsubscribePushCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.UserPushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == request.Endpoint, cancellationToken);

        if (subscription != null)
        {
            _context.UserPushSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
