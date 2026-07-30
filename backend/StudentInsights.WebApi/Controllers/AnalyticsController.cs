using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Enums;
using StudentInsights.Application.Features.Analytics.Queries.GetAcademicAnalytics;
using StudentInsights.Application.Features.Analytics.Queries.GetAssignmentProgress;
using StudentInsights.Application.Features.Analytics.Queries.GetGoalProgress;
using StudentInsights.Application.Features.Analytics.Queries.GetStudyTime;
using StudentInsights.Application.Features.Analytics.Queries.GetWeeklyActivity;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Exposes read-only performance analytics for the authenticated user --
/// completion/progress metrics aggregated over Courses, LearningActivities,
/// StudyLogs, and Goals, shaped for direct Chart.js rendering. Analytics
/// has no entity or CRUD surface of its own (same architectural
/// classification as Calendar/Dashboard): all aggregation logic lives in
/// the corresponding query handler, this controller only translates HTTP
/// requests into MediatR queries and results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the current user's assignment/project completion breakdown,
    /// optionally scoped to a due-date range.
    /// </summary>
    /// <param name="from">Optional inclusive lower bound on due date (UTC).</param>
    /// <param name="to">Optional inclusive upper bound on due date (UTC).</param>
    /// <returns>Completed/pending counts, completion rate, and a Chart.js-ready breakdown.</returns>
    /// <response code="200">The breakdown was retrieved.</response>
    /// <response code="400">'from' is later than 'to'.</response>
    [HttpGet("assignment-progress")]
    public async Task<ActionResult<AssignmentProgressDto>> GetAssignmentProgress(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAssignmentProgressQuery(from, to), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's goals with their computed progress
    /// (reusing the same GoalProgressCalculator/GoalProgressInputsProvider
    /// the Goals module itself uses), shaped as a Chart.js-ready snapshot.
    /// </summary>
    /// <returns>Each goal's progress plus a Chart.js-ready breakdown.</returns>
    /// <response code="200">The progress snapshot was retrieved.</response>
    [HttpGet("goal-progress")]
    public async Task<ActionResult<GoalProgressDto>> GetGoalProgress(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGoalProgressQuery(), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a day-by-day count of the current user's completed
    /// LearningActivities for the requested (or current) week.
    /// </summary>
    /// <param name="weekStartDate">
    /// Optional date within the desired week (UTC). Any day of that week
    /// resolves to the same result, since it's floored to the week's
    /// actual start via WeekBoundary. Omit for the current week.
    /// </param>
    /// <returns>The resolved week's start date and a Chart.js-ready daily breakdown.</returns>
    /// <response code="200">The weekly activity was retrieved.</response>
    [HttpGet("weekly-activity")]
    public async Task<ActionResult<WeeklyActivityDto>> GetWeeklyActivity(
        [FromQuery] DateTime? weekStartDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWeeklyActivityQuery(weekStartDate), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's study time bucketed by day, week, or
    /// month, optionally scoped to a study-date range.
    /// </summary>
    /// <param name="granularity">Required bucket size: Daily, Weekly, or Monthly.</param>
    /// <param name="from">Optional inclusive lower bound on study date (UTC).</param>
    /// <param name="to">Optional inclusive upper bound on study date (UTC).</param>
    /// <returns>Total minutes and a Chart.js-ready breakdown at the requested granularity.</returns>
    /// <response code="200">The study time breakdown was retrieved.</response>
    /// <response code="400">'granularity' was omitted or invalid, or 'from' is later than 'to'.</response>
    [HttpGet("study-time")]
    public async Task<ActionResult<StudyTimeDto>> GetStudyTime(
        [FromQuery] StudyTimeGranularity? granularity,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudyTimeQuery(granularity, from, to), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's academic snapshot: credit-weighted GPA
    /// (via the same GpaCalculator the Goals module already uses),
    /// average grade, and a per-course final-grade breakdown. Optional
    /// Phase 10.5 extension, not part of the four-endpoint MVP -- kept
    /// consistent with the other endpoints regardless, since it's still
    /// a first-class part of the shipped API surface.
    /// </summary>
    /// <returns>GPA/average-grade summary values plus a Chart.js-ready per-course breakdown.</returns>
    /// <response code="200">The academic snapshot was retrieved.</response>
    [HttpGet("academic")]
    public async Task<ActionResult<AcademicAnalyticsDto>> GetAcademicAnalytics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAcademicAnalyticsQuery(), cancellationToken);

        return Ok(result);
    }
}