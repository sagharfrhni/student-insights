using MediatR;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.Commands.CreateGoal;

/// <summary>
/// UserId is deliberately absent -- same convention as
/// CreateLearningActivityCommand, it is never accepted from client
/// input. CreateGoalCommandHandler resolves it from ICurrentUserService.
/// </summary>
public record CreateGoalCommand(
    GoalType Type,
    decimal TargetValue,
    DateTime? TargetDateUtc,
    Guid? RelatedActivityId) : IRequest<GoalDto>;