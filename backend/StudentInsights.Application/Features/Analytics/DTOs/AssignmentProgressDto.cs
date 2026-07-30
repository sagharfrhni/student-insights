namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// Read model for the Assignment Progress chart. Completed/Pending count
/// every LearningActivity regardless of ActivityType (Assignment and
/// Project are combined per the roadmap's Assignment Analytics
/// definition) -- this is a coarser view than Dashboard's
/// ActiveAssignmentsCount/ActiveProjectsCount split, which intentionally
/// separates the two.
/// </summary>
public record AssignmentProgressDto(
    int Completed,
    int Pending,
    double CompletionRatePercentage,
    ChartDatasetDto Chart);