namespace StudentInsights.Application.Features.Goals.Enums;

public enum GoalProgressStatus
{
    /// <summary>ProgressPercentage is meaningful and can be displayed.</summary>
    Available,

    /// <summary>
    /// The goal type is supported, but the data it needs doesn't exist
    /// yet for this user (e.g. GradePointAverage with no graded courses,
    /// or a ProjectDeadline goal whose related activity was soft-deleted).
    /// </summary>
    NotYetAvailable,

    /// <summary>
    /// No specific calculation exists for this goal type and no generic
    /// CurrentValue/TargetValue data applies either. Reserved as a safety
    /// net for future GoalType values; not reachable by any type defined
    /// today.
    /// </summary>
    NotSupportedYet
}