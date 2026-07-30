using MediatR;
using StudentInsights.Application.Features.Dashboard.DTOs;

namespace StudentInsights.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// No parameters -- Dashboard always summarizes the current user's own
/// data (from ICurrentUserService), so there's nothing for the caller to
/// specify. No validator is registered for it, for the same reason
/// ValidationBehavior already handles requests with none: there's
/// nothing to validate.
/// </summary>
public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;