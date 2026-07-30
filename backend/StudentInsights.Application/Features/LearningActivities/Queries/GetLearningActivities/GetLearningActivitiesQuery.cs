using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivities;

/// <summary>
/// Semester filters via the referenced Course's own Semester (LearningActivity
/// has no Semester of its own — it belongs to whichever term its Course
/// does), same reasoning and placement as GetExamsQuery's CourseId/Semester.
/// </summary>
public record GetLearningActivitiesQuery(
    Guid? CourseId,
    string? Semester,
    ActivityStatus? Status,
    ActivityType? Type,
    PaginationParams Pagination) : IRequest<PaginatedResult<LearningActivityDto>>;