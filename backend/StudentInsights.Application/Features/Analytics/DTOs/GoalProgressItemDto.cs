using StudentInsights.Application.Features.Goals.Enums;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Analytics.DTOs;

/// <summary>
/// One goal's computed progress, as returned by GoalProgressCalculator --
/// deliberately keeps Status alongside the nullable ProgressPercentage,
/// mirroring GoalDto, so a caller can tell "0% progress" (Available)
/// apart from "not yet available" (e.g. no graded courses for a GPA
/// goal) instead of both collapsing into the same 0. The chart's own
/// series still needs a concrete number for every point (see
/// AnalyticsMappingExtensions.ToGoalProgressDto), which is where the
/// null gets resolved to 0 -- not here.
/// </summary>
public record GoalProgressItemDto(
    Guid GoalId,
    GoalType Type,
    GoalProgressStatus Status,
    decimal? ProgressPercentage);