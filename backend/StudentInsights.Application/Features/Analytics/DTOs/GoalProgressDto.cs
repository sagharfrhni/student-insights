namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// Read model for the Goal Progress chart. MVP trend is a single
/// current-snapshot bar per goal (percentage complete), not a historical
/// time series -- the project has no goal-progress-history table yet
/// (see the roadmap's §6, flagged as a post-MVP extension), so "trend"
/// here means "where each goal stands right now", not "over time".
/// </summary>
public record GoalProgressDto(
    IReadOnlyList<GoalProgressItemDto> Goals,
    ChartDatasetDto Chart);