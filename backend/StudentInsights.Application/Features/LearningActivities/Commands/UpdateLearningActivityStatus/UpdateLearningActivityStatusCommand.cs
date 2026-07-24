using MediatR;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivityStatus;

public record UpdateLearningActivityStatusCommand(
    Guid LearningActivityId,
    ActivityStatus NewStatus) : IRequest<LearningActivityDto>;