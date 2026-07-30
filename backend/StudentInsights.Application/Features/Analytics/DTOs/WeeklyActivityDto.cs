namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// Read model for the Weekly Activity chart: a day-by-day count of
/// LearningActivity completions across the requested (or current) week.
/// WeekStartUtc is the actual, WeekBoundary-resolved start of that week --
/// included so a caller who passed an arbitrary WeekStartDate can see
/// exactly which week the chart ended up covering, the same reason
/// StudyTimeDto carries its own summary scalars alongside the chart.
/// </summary>
public record WeeklyActivityDto(
    DateTime WeekStartUtc,
    ChartDatasetDto Chart);