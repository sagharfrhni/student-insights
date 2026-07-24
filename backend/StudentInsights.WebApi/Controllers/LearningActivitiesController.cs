using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.LearningActivities.Commands.CreateLearningActivity;
using StudentInsights.Application.Features.LearningActivities.Commands.DeleteLearningActivity;
using StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivity;
using StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivityStatus;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivities;
using StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivityById;
using StudentInsights.Domain.Enums;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Manages the authenticated user's learning activities (assignments and
/// projects) belonging to their courses. Every endpoint requires
/// authentication; ownership of a given LearningActivity — and, on
/// create, of the owning Course — is enforced inside the corresponding
/// Application-layer handler, not here. This controller contains no
/// business logic — it only translates HTTP requests into MediatR
/// commands/queries and MediatR results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/learning-activities")]
[Authorize]
public class LearningActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningActivitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new learning activity under one of the current user's courses.</summary>
    /// <param name="request">The activity's course, title, type, due date, priority, and optional description/resource link.</param>
    /// <returns>The created learning activity.</returns>
    /// <response code="201">The learning activity was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The referenced course does not exist or is not owned by the current user.</response>
    [HttpPost]
    public async Task<ActionResult<LearningActivityDto>> CreateLearningActivity(
        [FromBody] CreateLearningActivityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLearningActivityCommand(
            request.CourseId,
            request.Title,
            request.Type,
            request.DueDateUtc,
            request.Priority,
            request.Description,
            request.ResourceLink);

        var activity = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetLearningActivityById), new { id = activity.Id }, activity);
    }

    /// <summary>Gets a single learning activity owned by the current user.</summary>
    /// <param name="id">The learning activity id.</param>
    /// <returns>The requested learning activity.</returns>
    /// <response code="200">The learning activity was found.</response>
    /// <response code="404">The learning activity does not exist or is not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LearningActivityDto>> GetLearningActivityById(Guid id, CancellationToken cancellationToken)
    {
        var activity = await _mediator.Send(new GetLearningActivityByIdQuery(id), cancellationToken);

        return Ok(activity);
    }

    /// <summary>Gets a paged, optionally filtered list of the current user's learning activities, ordered by due date.</summary>
    /// <param name="courseId">Optional course filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="type">Optional activity type filter.</param>
    /// <param name="pagination">Page number and page size.</param>
    /// <returns>A page of learning activities.</returns>
    /// <response code="200">The page was retrieved.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<LearningActivityDto>>> GetLearningActivities(
        [FromQuery] Guid? courseId,
        [FromQuery] ActivityStatus? status,
        [FromQuery] ActivityType? type,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetLearningActivitiesQuery(courseId, status, type, pagination);
        var activities = await _mediator.Send(query, cancellationToken);

        return Ok(activities);
    }

    /// <summary>Updates the descriptive details of a learning activity owned by the current user.</summary>
    /// <param name="id">The learning activity id.</param>
    /// <param name="request">The activity's new title, due date, priority, and optional description/resource link.</param>
    /// <returns>The updated learning activity.</returns>
    /// <response code="200">The learning activity was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The learning activity does not exist or is not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LearningActivityDto>> UpdateLearningActivity(
        Guid id,
        [FromBody] UpdateLearningActivityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLearningActivityCommand(
            id,
            request.Title,
            request.DueDateUtc,
            request.Priority,
            request.Description,
            request.ResourceLink);

        var activity = await _mediator.Send(command, cancellationToken);

        return Ok(activity);
    }

    /// <summary>Changes the status of a learning activity owned by the current user.</summary>
    /// <param name="id">The learning activity id.</param>
    /// <param name="request">The new status.</param>
    /// <returns>The updated learning activity.</returns>
    /// <response code="200">The status was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The learning activity does not exist or is not owned by the current user.</response>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<LearningActivityDto>> UpdateLearningActivityStatus(
        Guid id,
        [FromBody] UpdateLearningActivityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLearningActivityStatusCommand(id, request.NewStatus);
        var activity = await _mediator.Send(command, cancellationToken);

        return Ok(activity);
    }

    /// <summary>Deletes (soft-deletes) a learning activity owned by the current user.</summary>
    /// <param name="id">The learning activity id.</param>
    /// <response code="204">The learning activity was deleted.</response>
    /// <response code="404">The learning activity does not exist or is not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLearningActivity(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteLearningActivityCommand(id), cancellationToken);

        return NoContent();
    }
}