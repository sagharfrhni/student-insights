using MediatR;
using StudentInsights.Application.Features.Analytics.DTOs;

namespace StudentInsights.Application.Features.Analytics.Queries.GetAcademicAnalytics;

/// <summary>
/// No parameters -- Academic Analytics always summarizes all of the
/// current user's courses (from ICurrentUserService), the same
/// whole-user snapshot shape as GetGoalProgressQuery. No validator:
/// there's nothing to validate.
/// </summary>
public record GetAcademicAnalyticsQuery : IRequest<AcademicAnalyticsDto>;