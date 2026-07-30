using MediatR;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.Commands.CreateLearningActivity;

/// <summary>
/// UserId is deliberately absent — same convention as CreateCourseCommand,
/// it is never accepted from client input. CreateLearningActivityCommandHandler
/// resolves it from the owning Course via ICurrentUserService.
/// </summary>
public record CreateLearningActivityCommand(
    Guid CourseId,
    string Title,
    ActivityType Type,
    DateTime DueDateUtc,
    ActivityPriority Priority,
    string? Description,
    string? ResourceLink) : IRequest<LearningActivityDto>;