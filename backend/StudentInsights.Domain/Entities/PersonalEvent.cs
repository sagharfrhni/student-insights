using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.Entities;

public class PersonalEvent : BaseEntity
{
    private PersonalEvent()
    {
    } // EF Core

    private PersonalEvent(Guid userId, string title, string? description, DateTime startAtUtc, DateTime endAtUtc,
        bool isAllDay)
    {
        UserId = userId;
        Title = title;
        Description = description;
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        IsAllDay = isAllDay;
    }

    public static PersonalEvent Create(User user, string title, DateTime startAtUtc, DateTime endAtUtc,
        bool isAllDay = false, string? description = null)
    {
        if (user is null)
            throw new DomainException("User is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (endAtUtc <= startAtUtc)
            throw new DomainException("End time must be after the start time.");

        return new PersonalEvent(user.Id, title.Trim(), description?.Trim(), startAtUtc, endAtUtc, isAllDay);
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    /// <summary>Event title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; private set; }

    /// <summary>Event start time (UTC).</summary>
    public DateTime StartAtUtc { get; private set; }

    /// <summary>Event end time (UTC).</summary>
    public DateTime EndAtUtc { get; private set; }

    public bool IsAllDay { get; private set; }

    public void Reschedule(DateTime startAtUtc, DateTime endAtUtc)
    {
        if (endAtUtc <= startAtUtc)
            throw new DomainException("End time must be after the start time.");
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        MarkModified();
    }

    public void UpdateDetails(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        Title = title.Trim();
        Description = description?.Trim();
        MarkModified();
    }
}