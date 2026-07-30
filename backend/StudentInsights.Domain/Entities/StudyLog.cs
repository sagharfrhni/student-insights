using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.Entities;

public class StudyLog : BaseEntity
{
    /// <summary>
    /// Upper bound on a single session's duration ("12 hours" per product
    /// doc §3.9). Kept as a named constant on the entity itself — rather
    /// than a magic number duplicated in the validator — so both
    /// FluentValidation (shape-only check) and this Domain invariant
    /// (source of truth) read from one place.
    /// </summary>
    public const int MaxDurationMinutes = 720;

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
        EnsureValidDuration(durationMinutes);

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

    /// <summary>
    /// Only way to change the session date after creation. Mirrors
    /// Exam.Reschedule/LearningActivity.Reschedule. Unlike Create (which
    /// StudyDateUtc is validated against "not in the future" by
    /// CreateStudyLogCommandValidator), Update deliberately does not
    /// re-check that here — a student correcting an already-logged
    /// session shouldn't be blocked, the same asymmetry
    /// UpdateExamCommandValidator has versus CreateExamCommandValidator.
    /// </summary>
    public void Reschedule(DateTime studyDateUtc)
    {
        StudyDateUtc = studyDateUtc;
        MarkModified();
    }

    /// <summary>
    /// Only way to change the recorded duration after creation. Re-uses
    /// the same "positive, within MaxDurationMinutes" invariant enforced
    /// in Create, so the rule is defined once instead of drifting between
    /// the two call sites (same reasoning as Course.UpdateCredits).
    /// </summary>
    public void UpdateDuration(int durationMinutes)
    {
        EnsureValidDuration(durationMinutes);
        DurationMinutes = durationMinutes;
        MarkModified();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkModified();
    }

    /// <summary>
    /// Shared validation for Create and UpdateDuration — kept as one place
    /// so the two call sites can't drift apart (same pattern as
    /// User.EnsureValidToken).
    /// </summary>
    private static void EnsureValidDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new DomainException("Study duration must be greater than zero minutes.");
        if (durationMinutes > MaxDurationMinutes)
            throw new DomainException($"Study duration cannot exceed {MaxDurationMinutes} minutes.");
    }
}