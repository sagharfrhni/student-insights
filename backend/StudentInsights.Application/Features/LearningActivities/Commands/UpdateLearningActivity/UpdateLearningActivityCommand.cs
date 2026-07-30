using MediatR;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivity;

/// <summary>
/// CourseId and Type are deliberately absent: an activity is not
/// reassignable to a different course, and Type has no domain mutator
/// (see UpdateLearningActivityRequest for the full reasoning). Status and
/// CompletedAtUtc go through UpdateLearningActivityStatusCommand instead.
/// </summary>
public record UpdateLearningActivityCommand(
    Guid LearningActivityId,
    string Title,
    DateTime DueDateUtc,
    ActivityPriority Priority,
    string? Description,
    string? ResourceLink) : IRequest<LearningActivityDto>;