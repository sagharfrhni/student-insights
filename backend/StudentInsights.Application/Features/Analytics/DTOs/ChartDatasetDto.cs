namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// Shared Chart.js-ready shape reused by all four Analytics endpoints
/// (labels[] + datasets[]), so the React/Chart.js frontend renders every
/// chart with zero client-side transformation. Each endpoint's own DTO
/// (AssignmentProgressDto, GoalProgressDto, WeeklyActivityDto,
/// StudyTimeDto) embeds one of these plus whatever summary scalars are
/// specific to it (e.g. StudyTimeDto.TotalMinutes) -- this type only
/// carries the plottable series, never the summary numbers.
/// </summary>
public record ChartDatasetDto(
    IReadOnlyList<string> Labels,
    IReadOnlyList<ChartSeriesDto> Datasets);