using StudentInsights.Domain.Common;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Domain.Entities;

public class LearningActivity : BaseEntity
{
    private LearningActivity()
    {
    } // EF Core

    private LearningActivity(Guid userId, Guid courseId, string title, ActivityType type,
        DateTime dueDateUtc, ActivityPriority priority, string? description, string? resourceLink)
    {
        UserId = userId;
        CourseId = courseId;
        Title = title;
        Type = type;
        DueDateUtc = dueDateUtc;
        Priority = priority;
        Description = description;
        ResourceLink = resourceLink;
        Status = ActivityStatus.NotStarted;
    }

    /// <summary>
    /// Takes the owning Course (not raw IDs) so UserId/CourseId can never be
    /// set inconsistently with the course they belong to.
    /// </summary>
    public static LearningActivity Create(Course course, string title, ActivityType type, DateTime dueDateUtc,
        ActivityPriority priority = ActivityPriority.Medium, string? description = null, string? resourceLink = null)
    {
        if (course is null)
            throw new DomainException("Course is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        return new LearningActivity(course.UserId, course.Id, title.Trim(), type, dueDateUtc, priority,
            description?.Trim(), resourceLink?.Trim());
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    /// <summary>Assignment or project title.</summary>
    public string Title { get; private set; } = string.Empty;

    public ActivityType Type { get; private set; }

    /// <summary>Activity deadline (UTC).</summary>
    public DateTime DueDateUtc { get; private set; }

    public ActivityPriority Priority { get; private set; } = ActivityPriority.Medium;

    public ActivityStatus Status { get; private set; } = ActivityStatus.NotStarted;

    /// <summary>Optional description.</summary>
    public string? Description { get; private set; }

    /// <summary>Optional external resource link.</summary>
    public string? ResourceLink { get; private set; }

    /// <summary>Completion time (UTC).</summary>
    public DateTime? CompletedAtUtc { get; private set; }

    public void UpdateDetails(string title, string? description, string? resourceLink)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        Title = title.Trim();
        Description = description?.Trim();
        ResourceLink = resourceLink?.Trim();
        MarkModified();
    }

    public void Reschedule(DateTime dueDateUtc)
    {
        DueDateUtc = dueDateUtc;
        MarkModified();
    }

    public void SetPriority(ActivityPriority priority)
    {
        Priority = priority;
        MarkModified();
    }

    public void Start()
    {
        if (Status == ActivityStatus.Completed)
            throw new DomainException("A completed activity cannot be started; reopen it first.");
        Status = ActivityStatus.InProgress;
        MarkModified();
    }

    public void Complete()
    {
        Status = ActivityStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        MarkModified();
    }

    public void Reopen()
    {
        Status = ActivityStatus.NotStarted;
        CompletedAtUtc = null;
        MarkModified();
    }
}