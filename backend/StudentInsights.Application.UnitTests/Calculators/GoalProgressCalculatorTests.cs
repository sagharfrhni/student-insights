using StudentInsights.Application.Features.Goals.Enums;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Calculators;

public class GoalProgressCalculatorTests
{
    private static User ValidUser() =>
        User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");

    // --- GradePointAverage ---

    [Fact]
    public void CalculateProgress_Gpa_NoGpaYet_ReturnsNotYetAvailable()
    {
        var goal = Goal.Create(ValidUser(), GoalType.GradePointAverage, 18m);
        var inputs = new GoalProgressInputs(null, 0, null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.NotYetAvailable);
        result.ProgressPercentage.Should().BeNull();
    }

    [Fact]
    public void CalculateProgress_Gpa_WithGpa_ReturnsAvailableWithPercentage()
    {
        var goal = Goal.Create(ValidUser(), GoalType.GradePointAverage, 18m);
        var inputs = new GoalProgressInputs(9m, 0, null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.Available);
        result.ProgressPercentage.Should().Be(50m);
    }

    // --- StudyHours ---

    [Fact]
    public void CalculateProgress_StudyHours_ConvertsMinutesToHoursExactly()
    {
        // 90 minutes / 60 must yield exactly 1.5 hours — catches a stray
        // integer-division bug on a non-round-hour input.
        var goal = Goal.Create(ValidUser(), GoalType.StudyHours, 3m);
        var inputs = new GoalProgressInputs(null, 90, null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.Available);
        result.ProgressPercentage.Should().Be(50m); // 1.5h / 3h target
    }

    // --- ProjectDeadline ---

    [Theory]
    [InlineData(ActivityStatus.NotStarted, 0)]
    [InlineData(ActivityStatus.InProgress, 50)]
    [InlineData(ActivityStatus.Completed, 100)]
    public void CalculateProgress_ProjectDeadline_MapsActivityStatusToPercentage(
        ActivityStatus status, decimal expectedPercentage)
    {
        var user = ValidUser();
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        var activity = LearningActivity.Create(course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7));
        var goal = Goal.Create(user, GoalType.ProjectDeadline, 100m, relatedActivity: activity);
        var inputs = new GoalProgressInputs(null, 0, status);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.Available);
        result.ProgressPercentage.Should().Be(expectedPercentage);
    }

    [Fact]
    public void CalculateProgress_ProjectDeadline_NoRelatedActivityStatus_ReturnsNotYetAvailable()
    {
        var user = ValidUser();
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        var activity = LearningActivity.Create(course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7));
        var goal = Goal.Create(user, GoalType.ProjectDeadline, 100m, relatedActivity: activity);
        var inputs = new GoalProgressInputs(null, 0, RelatedActivityStatus: null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.NotYetAvailable);
    }

    // --- ChapterCount ---

    [Fact]
    public void CalculateProgress_ChapterCount_UsesGoalCurrentValue()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 10m, currentValue: 4m);
        var inputs = new GoalProgressInputs(null, 0, null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.Available);
        result.ProgressPercentage.Should().Be(40m);
    }

    [Theory]
    [InlineData(GoalType.ChapterCount, true)]
    [InlineData(GoalType.GradePointAverage, false)]
    [InlineData(GoalType.StudyHours, false)]
    [InlineData(GoalType.ProjectDeadline, false)]
    public void IsManuallyTracked_ReturnsExpectedFlag(GoalType type, bool expected)
    {
        GoalProgressCalculator.IsManuallyTracked(type).Should().Be(expected);
    }

    // --- Unmatched enum fallback ---

    [Fact]
    public void CalculateProgress_UnsupportedGoalType_ReturnsNotSupportedYet()
    {
        var goal = Goal.Create(ValidUser(), (GoalType)99, 10m);
        var inputs = new GoalProgressInputs(null, 0, null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        result.Status.Should().Be(GoalProgressStatus.NotSupportedYet);
        result.ProgressPercentage.Should().BeNull();
    }

    // --- CalculatePercentage edge cases (exercised via ChapterCount, the simplest carrier) ---

    [Fact]
    public void CalculateProgress_CurrentValueExceedsTarget_ClampsAt100()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 1m, currentValue: 5m);
        var inputs = new GoalProgressInputs(null, 0, null);

        var result = GoalProgressCalculator.CalculateProgress(goal, inputs);

        // currentValue (5) > targetValue (1) must clamp to 100, not exceed it.
        result.ProgressPercentage.Should().Be(100m);
    }
}