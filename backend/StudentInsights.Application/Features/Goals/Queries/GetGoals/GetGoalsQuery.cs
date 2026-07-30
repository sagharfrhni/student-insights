using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Goals.DTOs;

namespace StudentInsights.Application.Features.Goals.Queries.GetGoals;

/// <summary>
/// No filters beyond pagination -- the roadmap's Goals endpoint table
/// specifies only GET /goals with no query parameters, unlike
/// LearningActivities/Exams. Kept as simple as GetCoursesQuery rather than
/// speculatively adding a Type filter no spec calls for.
/// </summary>
public record GetGoalsQuery(PaginationParams Pagination) : IRequest<PaginatedResult<GoalDto>>;