using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.Entities;

/// <summary>
/// True child of the Course aggregate — created only via Course.AddClassSchedule
/// and removed only via Course.RemoveClassSchedule. Inherits AuditableEntity
/// (not BaseEntity): a class schedule has no business need for a soft-delete
/// history, so removal from the Course's collection is a real (hard) delete,
/// consistent with how EF Core will map an owned/dependent collection.
/// </summary>
public class ClassSchedule : AuditableEntity
{
    private ClassSchedule()
    {
    } // EF Core

    private ClassSchedule(Guid courseId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        CourseId = courseId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Internal by design: only Course.AddClassSchedule may create one, since
    /// that's where the end-after-start and no-overlap invariants live.
    /// </summary>
    internal static ClassSchedule Create(Course course, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        => new(course.Id, dayOfWeek, startTime, endTime);

    public Guid CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    /// <summary>Day of week the class recurs on.</summary>
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>Class start time.</summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>Class end time.</summary>
    public TimeSpan EndTime { get; private set; }
}