using StudentInsights.Domain.Common;
using StudentInsights.Domain.ValueObjects;

namespace StudentInsights.Domain.Entities;

public class Exam : BaseEntity
{
    private Exam()
    {
    } // EF Core

    private Exam(Guid userId, Guid courseId, string title, DateTime examDateUtc, string? description)
    {
        UserId = userId;
        CourseId = courseId;
        Title = title;
        ExamDateUtc = examDateUtc;
        Description = description;
    }

    public static Exam Create(Course course, string title, DateTime examDateUtc, string? description = null)
    {
        if (course is null)
            throw new DomainException("Course is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        return new Exam(course.UserId, course.Id, title.Trim(), examDateUtc, description?.Trim());
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    /// <summary>Exam title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Exam date and time (UTC).</summary>
    public DateTime ExamDateUtc { get; private set; }

    /// <summary>Final exam grade (null until graded).</summary>
    public Grade? Grade { get; private set; }

    /// <summary>Optional notes.</summary>
    public string? Description { get; private set; }

    public void Reschedule(DateTime examDateUtc)
    {
        ExamDateUtc = examDateUtc;
        MarkModified();
    }

    /// <summary>
    /// Only way to change Title after creation. Re-uses the same
    /// "required, non-blank" invariant enforced in Create, mirroring
    /// Course.Rename so the rule is defined once per entity instead of
    /// drifting between the two call sites.
    /// </summary>
    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        Title = title.Trim();
        MarkModified();
    }

    /// <summary>
    /// Only way to change Description after creation. Mirrors
    /// Course.UpdateInstructor: no NotEmpty guard since Description is
    /// optional, just trims and allows null to clear it.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkModified();
    }

    public void RecordGrade(decimal grade)
    {
        Grade = new Grade(grade);
        MarkModified();
    }
}