namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// Read model for the optional Phase 10.5 Academic Analytics endpoint.
/// CurrentGpa reuses GpaCalculator -- the exact same credit-weighted
/// formula the Goals module's GPA-type progress already calls -- so this
/// number can never drift from what a GPA goal reports elsewhere in the
/// app. CurrentGpa and AverageGrade are both null, not 0, when there are
/// no graded courses yet: a brand-new account's "no GPA yet" must never
/// look like "GPA of 0" (see AssignmentProgressDto's own divide-by-zero
/// guard for the same principle applied to a different metric).
/// </summary>
public record AcademicAnalyticsDto(
    decimal? CurrentGpa,
    decimal? AverageGrade,
    int GradedCoursesCount,
    int UngradedCoursesCount,
    ChartDatasetDto CourseStatistics);