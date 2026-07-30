using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.Entities;

public class StudyLog : BaseEntity
{
    private StudyLog()
    {
    } // EF Core

    private StudyLog(Guid userId, Guid courseId, DateTime studyDateUtc, int durationMinutes, string? notes)
    {
        UserId = userId;
        CourseId = courseId;
        StudyDateUtc = studyDateUtc;
        DurationMinutes = durationMinutes;
        Notes = notes;
    }

    public static StudyLog Create(Course course, DateTime studyDateUtc, int durationMinutes, string? notes = null)
    {
        if (course is null)
            throw new DomainException("Course is required.");
        if (durationMinutes <= 0)
            throw new DomainException("Study duration must be greater than zero minutes.");

        return new StudyLog(course.UserId, course.Id, studyDateUtc, durationMinutes, notes?.Trim());
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    /// <summary>Study session date (UTC).</summary>
    public DateTime StudyDateUtc { get; private set; }

    /// <summary>Study duration in minutes.</summary>
    public int DurationMinutes { get; private set; }

    /// <summary>Optional notes about the study session.</summary>
    public string? Notes { get; private set; }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkModified();
    }
}