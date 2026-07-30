using MediatR;
using StudentInsights.Application.Features.Goals.DTOs;

namespace StudentInsights.Application.Features.Goals.Queries.GetGoalById;

public record GetGoalByIdQuery(Guid GoalId) : IRequest<GoalDto>;