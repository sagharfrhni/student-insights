using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Domain;

public class GoalTests
{
    private static User ValidUser() =>
        User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");

    private static LearningActivity ActivityFor(User user)
    {
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        return LearningActivity.Create(course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7));
    }

    [Fact]
    public void Create_NonPositiveTargetValue_ThrowsDomainException()
    {
        var user = ValidUser();

        var act = () => Goal.Create(user, GoalType.ChapterCount, 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NegativeCurrentValue_ThrowsDomainException()
    {
        var user = ValidUser();

        var act = () => Goal.Create(user, GoalType.ChapterCount, 10m, currentValue: -1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ProjectDeadlineWithoutRelatedActivity_ThrowsDomainException()
    {
        var user = ValidUser();

        var act = () => Goal.Create(user, GoalType.ProjectDeadline, 100m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ProjectDeadlineWithRelatedActivityOwnedByAnotherUser_ThrowsDomainException()
    {
        var owner = ValidUser();
        var otherUser = ValidUser();
        var activity = ActivityFor(otherUser);

        var act = () => Goal.Create(owner, GoalType.ProjectDeadline, 100m, relatedActivity: activity);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ProjectDeadlineWithRelatedActivityOwnedBySameUser_Succeeds()
    {
        var user = ValidUser();
        var activity = ActivityFor(user);

        var goal = Goal.Create(user, GoalType.ProjectDeadline, 100m, relatedActivity: activity);

        goal.RelatedActivityId.Should().Be(activity.Id);
    }

    [Theory]
    [InlineData(GoalType.GradePointAverage)]
    [InlineData(GoalType.StudyHours)]
    [InlineData(GoalType.ChapterCount)]
    public void Create_NonProjectDeadlineWithRelatedActivity_ThrowsDomainException(GoalType type)
    {
        // Asymmetric rule: only ProjectDeadline goals may reference an
        // activity. Every other type must reject one — the exact case the
        // roadmap flags as easy to accidentally skip.
        var user = ValidUser();
        var activity = ActivityFor(user);

        var act = () => Goal.Create(user, type, 10m, relatedActivity: activity);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateProgress_NegativeValue_ThrowsDomainException()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 10m);

        var act = () => goal.UpdateProgress(-1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateProgress_ValidValue_UpdatesCurrentValue()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 10m);

        goal.UpdateProgress(5m);

        goal.CurrentValue.Should().Be(5m);
    }

    [Fact]
    public void UpdateTarget_NonPositiveValue_ThrowsDomainException()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 10m);

        var act = () => goal.UpdateTarget(0m, null);

        act.Should().Throw<DomainException>();
    }
}