using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivities;

public record GetLearningActivitiesQuery(
    Guid? CourseId,
    ActivityStatus? Status,
    ActivityType? Type,
    PaginationParams Pagination) : IRequest<PaginatedResult<LearningActivityDto>>;