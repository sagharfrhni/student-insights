using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Features.Dashboard.DTOs;
using StudentInsights.Application.Features.Dashboard.Queries.GetDashboardSummary;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Exposes a single read-only aggregate view of the authenticated user's
/// current term -- course/assignment/exam counts, goal progress, weekly
/// study time, recent activity, and unread notifications. Dashboard has
/// no entity or CRUD surface of its own (see CalendarController for the
/// same pattern), so this controller exposes one GET action and nothing
/// else. All aggregation logic lives in GetDashboardSummaryQueryHandler;
/// this controller only translates the HTTP request into a MediatR query
/// and the result into an HTTP response.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets a summarized view of the current user's academic term.</summary>
    /// <returns>Course/assignment/exam counts, goal progress, weekly study time, recent activity, and unread notification count.</returns>
    /// <response code="200">The summary was retrieved.</response>
    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var summary = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);

        return Ok(summary);
    }
}