using StudentInsights.Application.Features.Analytics.Enums;

namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// Read model for the Study Time chart. TotalMinutes is the sum across
/// the entire requested (or all-time) range -- not just the last bucket
/// -- so the frontend can show a headline number without re-summing
/// Chart.Datasets client-side.
/// </summary>
public record StudyTimeDto(
    StudyTimeGranularity Granularity,
    int TotalMinutes,
    ChartDatasetDto Chart);