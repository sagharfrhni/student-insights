using MediatR;
using StudentInsights.Application.Features.Goals.DTOs;

namespace StudentInsights.Application.Features.Goals.Commands.UpdateGoal;

/// <summary>
/// Type and RelatedActivityId are deliberately absent -- Goal exposes no
/// domain method that changes either after creation (see UpdateGoalRequest).
/// CurrentValue goes through UpdateGoalProgressCommand instead.
/// </summary>
public record UpdateGoalCommand(
    Guid GoalId,
    decimal TargetValue,
    DateTime? TargetDateUtc) : IRequest<GoalDto>;