using MediatR;
using StudentInsights.Application.Features.LearningActivities.DTOs;

namespace StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivityById;

public record GetLearningActivityByIdQuery(Guid LearningActivityId) : IRequest<LearningActivityDto>;