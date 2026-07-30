using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Calculators;

public class GoalProgressInputsProviderTests
{
    private static User ValidUser() =>
        User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");

    [Fact]
    public void BuildInputs_StudyHoursGoal_ExcludesLogsBeforeGoalCreatedAtUtc()
    {
        var goal = Goal.Create(ValidUser(), GoalType.StudyHours, 10m);

        var studyLogs = new List<(DateTime StudyDateUtc, int DurationMinutes)>
        {
            (goal.CreatedAtUtc.AddDays(-1), 120), // before goal was created — excluded
            (goal.CreatedAtUtc.AddMinutes(1), 30)  // after goal was created — included
        };

        var inputs = GoalProgressInputsProvider.BuildInputs(
            goal, null, studyLogs, new Dictionary<Guid, ActivityStatus>());

        inputs.StudyMinutesLoggedSinceGoalCreated.Should().Be(30);
    }

    [Fact]
    public void BuildInputs_NonStudyHoursGoal_StudyMinutesIsZero()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 10m);
        var studyLogs = new List<(DateTime StudyDateUtc, int DurationMinutes)>
        {
            (goal.CreatedAtUtc.AddMinutes(1), 60)
        };

        var inputs = GoalProgressInputsProvider.BuildInputs(
            goal, null, studyLogs, new Dictionary<Guid, ActivityStatus>());

        inputs.StudyMinutesLoggedSinceGoalCreated.Should().Be(0);
    }

    [Fact]
    public void BuildInputs_ProjectDeadlineGoal_RelatedActivityNotInBatch_ReturnsNullStatus()
    {
        var user = ValidUser();
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        var activity = LearningActivity.Create(course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7));
        var goal = Goal.Create(user, GoalType.ProjectDeadline, 100m, relatedActivity: activity);

        var inputs = GoalProgressInputsProvider.BuildInputs(
            goal,
            creditWeightedGpa: null,
            studyLogs: Array.Empty<(DateTime, int)>(),
            relatedActivityStatusesById: new Dictionary<Guid, ActivityStatus>()); // activity not resolved

        inputs.RelatedActivityStatus.Should().BeNull();
    }

    [Fact]
    public void BuildInputs_ProjectDeadlineGoal_RelatedActivityResolved_ReturnsItsStatus()
    {
        var user = ValidUser();
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        var activity = LearningActivity.Create(course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7));
        var goal = Goal.Create(user, GoalType.ProjectDeadline, 100m, relatedActivity: activity);
        var statusesById = new Dictionary<Guid, ActivityStatus> { [activity.Id] = ActivityStatus.InProgress };

        var inputs = GoalProgressInputsProvider.BuildInputs(
            goal, null, Array.Empty<(DateTime, int)>(), statusesById);

        inputs.RelatedActivityStatus.Should().Be(ActivityStatus.InProgress);
    }

    [Fact]
    public void BuildInputs_AlwaysPassesCreditWeightedGpaThrough()
    {
        var goal = Goal.Create(ValidUser(), GoalType.GradePointAverage, 18m);

        var inputs = GoalProgressInputsProvider.BuildInputs(
            goal, creditWeightedGpa: 15.5m, studyLogs: Array.Empty<(DateTime, int)>(), relatedActivityStatusesById: new Dictionary<Guid, ActivityStatus>());

        inputs.CreditWeightedGpa.Should().Be(15.5m);
    }
}