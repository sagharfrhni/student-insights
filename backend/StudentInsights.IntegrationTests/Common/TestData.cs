using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.IntegrationTests.Common;

/// <summary>
/// Static builders over the Domain's own factory methods (Course.Create,
/// User.Create, ...) — not a generic builder framework. Every method returns
/// a valid, ready-to-use entity via the real factory, so tests can never
/// build a state the Domain itself would reject.
///
/// Covers only the entities whose source has been shared so far. Extend this
/// class (do not create a second one) as Exam/PersonalEvent/StudyLog/
/// RefreshToken builders are needed in later stages.
/// </summary>
public static class TestData
{
    public static User ActiveStudent(string? email = null) =>
        User.Create(
            firstName: "Ali",
            lastName: "Rezaei",
            email: email ?? $"student.{Guid.NewGuid():N}@example.com",
            passwordHash: "test-hash",
            role: UserRole.Student);

    public static User ActiveAdmin(string? email = null) =>
        User.Create(
            firstName: "Admin",
            lastName: "User",
            email: email ?? $"admin.{Guid.NewGuid():N}@example.com",
            passwordHash: "test-hash",
            role: UserRole.Admin);

    public static Course CourseFor(
        User user,
        string name = "Database Systems",
        int credits = 3,
        string semester = "Fall 2026") =>
        Course.Create(user, name, credits, semester);

    public static LearningActivity LearningActivityFor(
        Course course,
        string title = "Assignment 1",
        ActivityType type = ActivityType.Assignment,
        DateTime? dueDateUtc = null) =>
        LearningActivity.Create(course, title, type, dueDateUtc ?? DateTime.UtcNow.AddDays(7));

    public static Goal ProjectDeadlineGoal(
        User user,
        LearningActivity relatedActivity,
        decimal targetValue = 100m) =>
        Goal.Create(user, GoalType.ProjectDeadline, targetValue, relatedActivity: relatedActivity);

    public static Goal ChapterCountGoal(
        User user,
        decimal targetValue = 10m,
        decimal currentValue = 0m) =>
        Goal.Create(user, GoalType.ChapterCount, targetValue, currentValue: currentValue);

    public static Goal StudyHoursGoal(User user, decimal targetValue = 20m) =>
        Goal.Create(user, GoalType.StudyHours, targetValue);
}