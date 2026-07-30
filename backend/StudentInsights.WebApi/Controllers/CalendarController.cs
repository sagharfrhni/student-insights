using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Features.Calendar.DTOs;
using StudentInsights.Application.Features.Calendar.Enums;
using StudentInsights.Application.Features.Calendar.Queries.GetCalendarEvents;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Exposes the authenticated user's merged calendar — classes, exams,
/// deadlines, dated goals, and personal events — as a single
/// chronologically sorted list. Calendar has no entity or CRUD surface of
/// its own (see the module's architectural classification: a read-side
/// aggregation over five existing sources), so unlike ExamsController /
/// CoursesController this controller exposes a single read endpoint and
/// no Create/Update/Delete actions. All merge/expansion logic lives in
/// GetCalendarEventsQueryHandler; this controller only translates the
/// HTTP request into a MediatR query and the result into an HTTP
/// response.
/// </summary>
[ApiController]
[Route("api/calendar")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;

    public CalendarController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the current user's calendar events within an inclusive date
    /// range, optionally restricted to a subset of event types.
    /// </summary>
    /// <param name="from">Inclusive lower bound (UTC).</param>
    /// <param name="to">Inclusive upper bound (UTC).</param>
    /// <param name="types">
    /// Optional set of event types to include (e.g. ?types=Exam,Deadline).
    /// Omit to include all types.
    /// </param>
    /// <returns>The matching events, sorted chronologically by start time.</returns>
    /// <response code="200">The events were retrieved.</response>
    /// <response code="400">'from' is later than 'to', or the range exceeds the maximum allowed span.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CalendarEventDto>>> GetCalendarEvents(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] CalendarEventType[]? types,
        CancellationToken cancellationToken)
    {
        var events = await _mediator.Send(new GetCalendarEventsQuery(from, to, types), cancellationToken);

        return Ok(events);
    }
}