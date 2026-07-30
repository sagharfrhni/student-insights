using StudentInsights.Domain.Common;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Domain.Entities;

public class Notification : BaseEntity
{
    private Notification()
    {
    } // EF Core

    private Notification(Guid userId, NotificationType type, string message, Guid? sourceId)
    {
        UserId = userId;
        Type = type;
        Message = message;
        SourceId = sourceId;
        IsRead = false;
    }

    public static Notification Create(User user, NotificationType type, string message, Guid? sourceId = null)
    {
        if (user is null)
            throw new DomainException("User is required.");
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Message is required.");

        return new Notification(user.Id, type, message.Trim(), sourceId);
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public NotificationType Type { get; private set; }

    /// <summary>Notification content.</summary>
    public string Message { get; private set; } = string.Empty;

    public bool IsRead { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    /// <summary>
    /// Id of the related entity (Exam, LearningActivity, Goal...) depending on
    /// Type. Intentionally untyped: Notification is a lightweight, generated
    /// read-model entity, not an aggregate root with invariants about what it
    /// points to. Interpretation happens in the Application layer, which is
    /// the one place that already knows how to map NotificationType to a
    /// source entity.
    /// </summary>
    public Guid? SourceId { get; private set; }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
        MarkModified();
    }
}