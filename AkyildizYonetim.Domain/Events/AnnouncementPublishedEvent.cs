using MediatR;

namespace AkyildizYonetim.Domain.Events;

public class AnnouncementPublishedEvent : INotification
{
    public string Title { get; }
    public string Message { get; }
    public DateTime OccurredAt { get; }

    public AnnouncementPublishedEvent(string title, string message)
    {
        Title = title;
        Message = message;
        OccurredAt = DateTime.UtcNow;
    }
}
