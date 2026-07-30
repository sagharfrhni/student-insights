using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Domain;

public class LearningActivityTests
{
    private static LearningActivity ValidActivity()
    {
        var user = User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        return LearningActivity.Create(course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7));
    }

    [Fact]
    public void Create_NullCourse_ThrowsDomainException()
    {
        var act = () => LearningActivity.Create(null!, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_BlankTitle_ThrowsDomainException()
    {
        var user = User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");

        var act = () => LearningActivity.Create(course, "  ", ActivityType.Assignment, DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_SetsCompletedAtUtcAndLastCompletedAtUtc()
    {
        var activity = ValidActivity();

        activity.Complete();

        activity.Status.Should().Be(ActivityStatus.Completed);
        activity.CompletedAtUtc.Should().NotBeNull();
        activity.LastCompletedAtUtc.Should().Be(activity.CompletedAtUtc);
    }

    [Fact]
    public void Reopen_ClearsCompletedAtUtc_ButNotLastCompletedAtUtc()
    {
        var activity = ValidActivity();
        activity.Complete();
        var lastCompleted = activity.LastCompletedAtUtc;

        activity.Reopen();

        activity.Status.Should().Be(ActivityStatus.NotStarted);
        activity.CompletedAtUtc.Should().BeNull();
        activity.LastCompletedAtUtc.Should().Be(lastCompleted);
    }

    [Fact]
    public void Start_OnCompletedActivity_ThrowsDomainException()
    {
        var activity = ValidActivity();
        activity.Complete();

        var act = activity.Start;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Start_OnNotStartedActivity_SetsInProgress()
    {
        var activity = ValidActivity();

        activity.Start();

        activity.Status.Should().Be(ActivityStatus.InProgress);
    }

    [Fact]
    public void UpdateDetails_BlankTitle_ThrowsDomainException()
    {
        var activity = ValidActivity();

        var act = () => activity.UpdateDetails("  ", null, null);

        act.Should().Throw<DomainException>();
    }
}