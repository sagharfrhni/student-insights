// StudentInsights.WebApi/Controllers/PersonalEventsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.PersonalEvents.Commands.CreatePersonalEvent;
using StudentInsights.Application.Features.PersonalEvents.Commands.DeletePersonalEvent;
using StudentInsights.Application.Features.PersonalEvents.Commands.UpdatePersonalEvent;
using StudentInsights.Application.Features.PersonalEvents.DTOs;
using StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEventById;
using StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEvents;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Manages the authenticated user's personal calendar events. Every
/// endpoint requires authentication; ownership of a given PersonalEvent
/// is enforced inside the corresponding Application-layer handler, not
/// here. This controller contains no business logic — it only translates
/// HTTP requests into MediatR commands/queries and MediatR results into
/// HTTP responses.
/// </summary>
[ApiController]
[Route("api/personal-events")]
[Authorize]
public class PersonalEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonalEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new personal event owned by the current user.</summary>
    /// <param name="request">The event's title, time window, all-day flag, and description.</param>
    /// <returns>The created personal event.</returns>
    /// <response code="201">The event was created.</response>
    /// <response code="400">The request failed validation.</response>
    [HttpPost]
    public async Task<ActionResult<PersonalEventDto>> CreatePersonalEvent(
        [FromBody] CreatePersonalEventRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePersonalEventCommand(
            request.Title,
            request.StartAtUtc,
            request.EndAtUtc,
            request.IsAllDay,
            request.Description);

        var personalEvent = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetPersonalEventById), new { id = personalEvent.Id }, personalEvent);
    }

    /// <summary>Gets a single personal event owned by the current user.</summary>
    /// <param name="id">The personal event id.</param>
    /// <returns>The requested personal event.</returns>
    /// <response code="200">The event was found.</response>
    /// <response code="404">The event does not exist or is not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonalEventDto>> GetPersonalEventById(Guid id, CancellationToken cancellationToken)
    {
        var personalEvent = await _mediator.Send(new GetPersonalEventByIdQuery(id), cancellationToken);

        return Ok(personalEvent);
    }

    /// <summary>
    /// Gets a paged list of the current user's personal events, soonest
    /// first, optionally filtered by a start-time date range.
    /// </summary>
    /// <param name="pagination">Page number and page size.</param>
    /// <param name="from">Optional inclusive lower bound on start time (UTC).</param>
    /// <param name="to">Optional inclusive upper bound on start time (UTC).</param>
    /// <returns>A page of personal events.</returns>
    /// <response code="200">The page was retrieved.</response>
    /// <response code="400">'from' is later than 'to'.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PersonalEventDto>>> GetPersonalEvents(
        [FromQuery] PaginationParams pagination,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var personalEvents = await _mediator.Send(new GetPersonalEventsQuery(pagination, from, to), cancellationToken);

        return Ok(personalEvents);
    }

    /// <summary>Updates a personal event owned by the current user.</summary>
    /// <param name="id">The personal event id.</param>
    /// <param name="request">The event's new title, time window, and description.</param>
    /// <returns>The updated personal event.</returns>
    /// <response code="200">The event was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The event does not exist or is not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PersonalEventDto>> UpdatePersonalEvent(
        Guid id,
        [FromBody] UpdatePersonalEventRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePersonalEventCommand(id, request.Title, request.StartAtUtc, request.EndAtUtc, request.Description);
        var personalEvent = await _mediator.Send(command, cancellationToken);

        return Ok(personalEvent);
    }

    /// <summary>Deletes (soft-deletes) a personal event owned by the current user.</summary>
    /// <param name="id">The personal event id.</param>
    /// <response code="204">The event was deleted.</response>
    /// <response code="404">The event does not exist or is not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePersonalEvent(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeletePersonalEventCommand(id), cancellationToken);

        return NoContent();
    }
}