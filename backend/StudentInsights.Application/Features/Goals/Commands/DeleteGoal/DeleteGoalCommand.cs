using MediatR;

namespace StudentInsights.Application.Features.Goals.Commands.DeleteGoal;

public record DeleteGoalCommand(Guid GoalId) : IRequest;