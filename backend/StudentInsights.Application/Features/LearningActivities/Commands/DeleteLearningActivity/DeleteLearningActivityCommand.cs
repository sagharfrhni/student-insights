using MediatR;

namespace StudentInsights.Application.Features.LearningActivities.Commands.DeleteLearningActivity;

public record DeleteLearningActivityCommand(Guid LearningActivityId) : IRequest;