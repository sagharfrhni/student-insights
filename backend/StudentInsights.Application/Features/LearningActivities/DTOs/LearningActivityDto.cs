using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.DTOs;

/// <summary>
/// Read model for a LearningActivity, returned from queries. CourseName is
/// included for display purposes so API consumers don't need a separate
/// lookup against /courses/{courseId} to render a task list; it is
/// projected in by the query/mapping, not read off a loaded Course
/// navigation, to avoid forcing an Include on every handler (see
/// LearningActivityMappingExtensions).
/// </summary>
public record LearningActivityDto(
    Guid Id,
    Guid CourseId,
    string CourseName,
    string Title,
    ActivityType Type,
    DateTime DueDateUtc,
    ActivityPriority Priority,
    ActivityStatus Status,
    string? Description,
    string? ResourceLink,
    DateTime? CompletedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);