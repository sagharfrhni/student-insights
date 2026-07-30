using MediatR;
using StudentInsights.Application.Features.Goals.DTOs;

namespace StudentInsights.Application.Features.Goals.Commands.UpdateGoalProgress;

public record UpdateGoalProgressCommand(Guid GoalId, decimal CurrentValue) : IRequest<GoalDto>;