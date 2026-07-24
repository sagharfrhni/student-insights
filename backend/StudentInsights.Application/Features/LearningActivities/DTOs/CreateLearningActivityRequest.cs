using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.DTOs;

/// <summary>
/// User-supplied fields for creating a LearningActivity. UserId is
/// intentionally absent — same convention as CreateCourseRequest, it is
/// always resolved server-side from ICurrentUserService. Status is also
/// absent: a new activity always starts as NotStarted (enforced by
/// LearningActivity.Create), and CompletedAtUtc is system-managed and can
/// only ever be set by completing the activity.
/// </summary>
public record CreateLearningActivityRequest(
    Guid CourseId,
    string Title,
    ActivityType Type,
    DateTime DueDateUtc,
    ActivityPriority Priority,
    string? Description,
    string? ResourceLink);