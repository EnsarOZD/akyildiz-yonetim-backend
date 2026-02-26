using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Events;
using MediatR;

namespace AkyildizYonetim.Application.Notifications.Commands.PostAnnouncement;

public record PostAnnouncementCommand : IRequest<Result>
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class PostAnnouncementCommandHandler : IRequestHandler<PostAnnouncementCommand, Result>
{
    private readonly IMediator _mediator;

    public PostAnnouncementCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result> Handle(PostAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            return Result.Failure("Başlık ve mesaj boş olamaz.");

        // Trigger the broadcast event
        await _mediator.Publish(new AnnouncementPublishedEvent(request.Title, request.Message), cancellationToken);

        return Result.Success();
    }
}
