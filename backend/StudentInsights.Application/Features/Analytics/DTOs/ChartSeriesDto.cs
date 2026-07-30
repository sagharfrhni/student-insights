namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// One Chart.js-ready data series. Data is non-nullable double: a goal
/// whose progress isn't computable yet (see GoalProgressStatus) is
/// rendered as 0 here, while the accompanying GoalProgressItemDto still
/// carries the real nullable percentage and status for anything that
/// needs to tell "0% progress" apart from "not yet available".
/// </summary>
public record ChartSeriesDto(
    string Label,
    IReadOnlyList<double> Data);