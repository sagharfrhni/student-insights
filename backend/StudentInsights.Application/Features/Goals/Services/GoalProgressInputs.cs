using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.Services;

/// <summary>
/// Already-fetched data GoalProgressCalculator needs, batched by the
/// caller (see GetDashboardSummaryQueryHandler) so the calculator itself
/// does zero I/O and stays a pure, unit-testable function.
/// </summary>
/// <param name="CreditWeightedGpa">
/// From GpaCalculator.CalculateCreditWeighted -- null when the user has
/// no graded course yet.
/// </param>
/// <param name="StudyMinutesLoggedSinceGoalCreated">
/// Sum of StudyLog.DurationMinutes for the user, filtered to
/// StudyDateUtc >= the Goal's own CreatedAtUtc. 0, not null, when there
/// are no matching logs -- that's a real "no progress yet" answer, not
/// missing data.
/// </param>
/// <param name="RelatedActivityStatus">
/// Null unless the goal is a ProjectDeadline goal whose RelatedActivityId
/// still resolves to a (non-soft-deleted) LearningActivity owned by the
/// same user.
/// </param>
public record GoalProgressInputs(
    decimal? CreditWeightedGpa,
    int StudyMinutesLoggedSinceGoalCreated,
    ActivityStatus? RelatedActivityStatus);