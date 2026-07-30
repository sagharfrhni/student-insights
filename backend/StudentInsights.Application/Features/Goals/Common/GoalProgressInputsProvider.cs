using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Academics;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.Common;

/// <summary>
/// Gathers the already-fetched data GoalProgressCalculator needs, for
/// either a single Goal or a whole batch, running only the query (or
/// queries) each present GoalType actually requires. Static and takes
/// IApplicationDbContext as an explicit parameter rather than a
/// constructor-injected service -- same convention as
/// PaginatedResult{T}.CreateAsync -- since this is a stateless read over
/// data the caller already has a context for.
/// </summary>
public static class GoalProgressInputsProvider
{
    /// <summary>
    /// Single-goal convenience wrapper around GetBatchAsync (used by
    /// Create/Update/UpdateProgress, which each operate on one Goal at a
    /// time). Delegating here instead of duplicating the per-type
    /// branching keeps "what data does GoalType X need" defined in
    /// exactly one place.
    /// </summary>
    public static async Task<GoalProgressInputs> GetAsync(
        IApplicationDbContext context, Guid userId, Goal goal, CancellationToken cancellationToken)
    {
        var batch = await GetBatchAsync(context, userId, new[] { goal }, cancellationToken);
        return batch[goal.Id];
    }

    /// <summary>
    /// Batched for a whole page of goals (see GetGoalsQueryHandler): at
    /// most three queries total no matter how many goals are passed in --
    /// one for credit-weighted GPA, one for the user's study logs, and one
    /// for related-activity statuses -- each only run if at least one goal
    /// in the batch actually needs it. Per-goal values are resolved via
    /// BuildInputs from the shared, already-fetched data instead of one
    /// query per goal.
    /// </summary>
    public static async Task<IReadOnlyDictionary<Guid, GoalProgressInputs>> GetBatchAsync(
        IApplicationDbContext context, Guid userId, IReadOnlyCollection<Goal> goals, CancellationToken cancellationToken)
    {
        decimal? creditWeightedGpa = null;
        if (goals.Any(g => g.Type == GoalType.GradePointAverage))
        {
            // Scoped to the user's current semester (see
            // CurrentSemesterCourseProvider's remarks for what "current"
            // means) rather than a lifetime average across every graded
            // course the user has ever entered -- otherwise a GPA goal
            // silently blends every semester together the moment a student
            // has used the app for more than one term.
            creditWeightedGpa = await CurrentSemesterCourseProvider.GetCurrentSemesterGpaAsync(
                context, userId, cancellationToken);
        }

        // Only logs recorded since a given goal's own CreatedAtUtc count
        // towards it (GoalProgressInputs' documented contract), so the raw
        // logs are fetched once here and summed per-goal in BuildInputs
        // instead of one SumAsync per StudyHours goal.
        IReadOnlyList<(DateTime StudyDateUtc, int DurationMinutes)> studyLogs =
            Array.Empty<(DateTime, int)>();
        if (goals.Any(g => g.Type == GoalType.StudyHours))
        {
            var rawLogs = await context.StudyLogs
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(s => new { s.StudyDateUtc, s.DurationMinutes })
                .ToListAsync(cancellationToken);

            studyLogs = rawLogs.Select(s => (s.StudyDateUtc, s.DurationMinutes)).ToList();
        }

        // A soft-deleted activity is filtered out by the global query
        // filter and simply won't appear in this dictionary, which
        // resolves to null (not-yet-available) in BuildInputs -- matching
        // GoalProgressCalculator's handling of a missing related activity.
        var relatedActivityIds = goals
            .Where(g => g.Type == GoalType.ProjectDeadline && g.RelatedActivityId is not null)
            .Select(g => g.RelatedActivityId!.Value)
            .Distinct()
            .ToList();

        var activityStatusesById = relatedActivityIds.Count == 0
            ? new Dictionary<Guid, ActivityStatus>()
            : await context.LearningActivities
                .AsNoTracking()
                .Where(la => relatedActivityIds.Contains(la.Id))
                .Select(la => new { la.Id, la.Status })
                .ToDictionaryAsync(la => la.Id, la => la.Status, cancellationToken);

        var result = new Dictionary<Guid, GoalProgressInputs>();
        foreach (var goal in goals)
        {
            result[goal.Id] = BuildInputs(goal, creditWeightedGpa, studyLogs, activityStatusesById);
        }

        return result;
    }

    /// <summary>
    /// Pure, no-I/O per-goal projection from already-fetched batch data
    /// into a GoalProgressInputs. This is the single place "which study
    /// logs count for goal X" / "which related-activity status counts for
    /// goal X" is decided -- shared by GetBatchAsync above AND by
    /// GetDashboardSummaryQueryHandler, which fetches its own courses/
    /// study-logs/activity-statuses for its own performance reasons
    /// (see that handler's comments) but must resolve them per goal
    /// exactly the same way GetBatchAsync does.
    /// </summary>
    public static GoalProgressInputs BuildInputs(
        Goal goal,
        decimal? creditWeightedGpa,
        IReadOnlyList<(DateTime StudyDateUtc, int DurationMinutes)> studyLogs,
        IReadOnlyDictionary<Guid, ActivityStatus> relatedActivityStatusesById)
    {
        var studyMinutesLogged = goal.Type == GoalType.StudyHours
            ? studyLogs.Where(s => s.StudyDateUtc >= goal.CreatedAtUtc).Sum(s => s.DurationMinutes)
            : 0;

        ActivityStatus? relatedActivityStatus =
            goal.Type == GoalType.ProjectDeadline
            && goal.RelatedActivityId is not null
            && relatedActivityStatusesById.TryGetValue(goal.RelatedActivityId.Value, out var status)
                ? status
                : null;

        return new GoalProgressInputs(creditWeightedGpa, studyMinutesLogged, relatedActivityStatus);
    }
}