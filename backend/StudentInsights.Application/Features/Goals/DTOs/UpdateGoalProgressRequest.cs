namespace StudentInsights.Application.Features.Goals.DTOs;

/// <summary>
/// User-supplied payload for the dedicated progress-update endpoint
/// (PATCH /goals/{id}/progress), mirroring the narrow, single-purpose
/// PATCH /{id}/status shape used by LearningActivities. Only valid for
/// goal types GoalProgressCalculator.IsManuallyTracked reports as true
/// (currently GoalType.ChapterCount) -- UpdateGoalProgressCommandHandler
/// rejects it for computed types instead of letting CurrentValue go
/// stale.
/// </summary>
public record UpdateGoalProgressRequest(decimal CurrentValue);