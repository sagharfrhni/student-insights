using MediatR;
using StudentInsights.Application.Features.Analytics.DTOs;

namespace StudentInsights.Application.Features.Analytics.Queries.GetGoalProgress;

/// <summary>
/// No parameters -- Goal Progress always summarizes the current user's
/// own goals (from ICurrentUserService), so there's nothing for the
/// caller to specify. Mirrors GetGoalsQuery/GetDashboardSummaryQuery: no
/// validator is registered for it, since there's nothing to validate.
/// </summary>
public record GetGoalProgressQuery : IRequest<GoalProgressDto>;