using StudentInsights.Application.Features.Goals.Enums;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.DTOs;

/// <summary>
/// Read model for a Goal with computed progress attached. Designed to be
/// promoted into a full Features/Goals module later without a breaking
/// shape change.
/// </summary>
public record GoalDto(
    Guid Id,
    GoalType Type,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime? TargetDateUtc,
    Guid? RelatedActivityId,
    GoalProgressStatus ProgressStatus,
    decimal? ProgressPercentage,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);