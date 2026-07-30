using StudentInsights.Domain.Common;
using StudentInsights.Domain.ValueObjects;

namespace StudentInsights.Domain.Entities;

// NOTE: Deleting a Course also soft-deletes its related LearningActivities,
// Exams, and StudyLogs. Since those are separate aggregates (see the
// comment on the collections below), that orchestration is intentionally
// NOT implemented here — it lives in DeleteCourseCommandHandler
// (Application layer), which loads each dependent collection from its own
// DbSet and soft-deletes it explicitly before soft-deleting the Course.
// ClassSchedules are NOT part of that cascade: they are part of the Course
// aggregate itself (not independently soft-deletable — see
// CourseConfiguration's Cascade FK) and are only removed if the Course row
// is ever hard-deleted.

public class Course : BaseEntity
{
    private readonly List<ClassSchedule> _classSchedules = new();
    private readonly List<LearningActivity> _learningActivities = new();
    private readonly List<Exam> _exams = new();
    private readonly List<StudyLog> _studyLogs = new();

    private Course()
    {
    } // EF Core

    private Course(Guid userId, string name, int credits, string? instructorName)
    {
        UserId = userId;
        Name = name;
        Credits = credits;
        InstructorName = instructorName;
    }

    public static Course Create(User user, string name, int credits, string? instructorName = null)
    {
        if (user is null)
            throw new DomainException("User is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Course name is required.");
        if (credits <= 0)
            throw new DomainException("Credits must be greater than zero.");

        return new Course(user.Id, name.Trim(), credits, instructorName?.Trim());
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    /// <summary>Course name (e.g. Database Systems).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Number of academic credits.</summary>
    public int Credits { get; private set; }

    /// <summary>Instructor full name.</summary>
    public string? InstructorName { get; private set; }

    /// <summary>Final grade (null until the course is completed).</summary>
    public Grade? FinalGrade { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Course name is required.");
        Name = name.Trim();
        MarkModified();
    }

    public void UpdateInstructor(string? instructorName)
    {
        InstructorName = instructorName?.Trim();
        MarkModified();
    }

    /// <summary>
    /// Only way to change Credits after creation. Re-uses the same
    /// invariant enforced in Create, so "credits must be positive" is
    /// defined once instead of drifting between the two call sites.
    /// </summary>
    public void UpdateCredits(int credits)
    {
        if (credits <= 0)
            throw new DomainException("Credits must be greater than zero.");
        Credits = credits;
        MarkModified();
    }

    public void SetFinalGrade(decimal grade)
    {
        FinalGrade = new Grade(grade);
        MarkModified();
    }

    /// <summary>
    /// Only way to add a class schedule. Enforces end-after-start and no
    /// overlap with the course's existing sessions — invariants that belong
    /// to the Course aggregate, not scattered across the Application layer.
    /// </summary>
    public ClassSchedule AddClassSchedule(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("Class end time must be after the start time.");

        var overlaps = _classSchedules.Any(s =>
            s.DayOfWeek == dayOfWeek && startTime < s.EndTime && endTime > s.StartTime);
        if (overlaps)
            throw new DomainException("This class schedule overlaps with an existing one for this course.");

        var schedule = ClassSchedule.Create(this, dayOfWeek, startTime, endTime);
        _classSchedules.Add(schedule);
        MarkModified();
        return schedule;
    }

    public void RemoveClassSchedule(ClassSchedule schedule)
    {
        _classSchedules.Remove(schedule);
        MarkModified();
    }

    public IReadOnlyCollection<ClassSchedule> ClassSchedules => _classSchedules;

    // LearningActivity/Exam/StudyLog are deliberately NOT created through
    // Course (e.g. no Course.AddLearningActivity). They're independent
    // aggregates with their own repositories — assignments are created,
    // edited and deleted individually and constantly, so requiring the
    // whole Course graph to be loaded for every one of those operations
    // would be wasteful. Their factories still take the Course object (see
    // LearningActivity.Create) so UserId/CourseId can never drift.
    public IReadOnlyCollection<LearningActivity> LearningActivities => _learningActivities;
    public IReadOnlyCollection<Exam> Exams => _exams;
    public IReadOnlyCollection<StudyLog> StudyLogs => _studyLogs;
}