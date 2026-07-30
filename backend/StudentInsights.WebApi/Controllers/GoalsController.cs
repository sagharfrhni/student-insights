using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Goals.Commands.CreateGoal;
using StudentInsights.Application.Features.Goals.Commands.DeleteGoal;
using StudentInsights.Application.Features.Goals.Commands.UpdateGoal;
using StudentInsights.Application.Features.Goals.Commands.UpdateGoalProgress;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Queries.GetGoalById;
using StudentInsights.Application.Features.Goals.Queries.GetGoals;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Manages the authenticated user's academic goals (GPA, study hours,
/// project deadlines, chapter counts) and their computed progress. Every
/// endpoint requires authentication; ownership of a given Goal — and, on
/// create, of a referenced LearningActivity for ProjectDeadline goals —
/// is enforced inside the corresponding Application-layer handler, not
/// here. This controller contains no business logic — it only translates
/// HTTP requests into MediatR commands/queries and MediatR results into
/// HTTP responses.
/// </summary>
[ApiController]
[Route("api/goals")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GoalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new goal for the current user.</summary>
    /// <param name="request">The goal's type, target value, optional target date, and (ProjectDeadline only) related activity.</param>
    /// <returns>The created goal, including its computed progress.</returns>
    /// <response code="201">The goal was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">RelatedActivityId was set but does not reference a learning activity owned by the current user.</response>
    [HttpPost]
    public async Task<ActionResult<GoalDto>> CreateGoal(
        [FromBody] CreateGoalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGoalCommand(
            request.Type,
            request.TargetValue,
            request.TargetDateUtc,
            request.RelatedActivityId);

        var goal = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetGoalById), new { id = goal.Id }, goal);
    }

    /// <summary>Gets a single goal owned by the current user, including its computed progress.</summary>
    /// <param name="id">The goal id.</param>
    /// <returns>The requested goal.</returns>
    /// <response code="200">The goal was found.</response>
    /// <response code="404">The goal does not exist or is not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GoalDto>> GetGoalById(Guid id, CancellationToken cancellationToken)
    {
        var goal = await _mediator.Send(new GetGoalByIdQuery(id), cancellationToken);

        return Ok(goal);
    }

    /// <summary>Gets a paged list of the current user's goals, newest first, including their computed progress.</summary>
    /// <param name="pagination">Page number and page size.</param>
    /// <returns>A page of goals.</returns>
    /// <response code="200">The page was retrieved.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GoalDto>>> GetGoals(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var goals = await _mediator.Send(new GetGoalsQuery(pagination), cancellationToken);

        return Ok(goals);
    }

    /// <summary>Updates the target value and target date of a goal owned by the current user.</summary>
    /// <param name="id">The goal id.</param>
    /// <param name="request">The goal's new target value and optional target date.</param>
    /// <returns>The updated goal.</returns>
    /// <response code="200">The goal was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The goal does not exist or is not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GoalDto>> UpdateGoal(
        Guid id,
        [FromBody] UpdateGoalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGoalCommand(id, request.TargetValue, request.TargetDateUtc);
        var goal = await _mediator.Send(command, cancellationToken);

        return Ok(goal);
    }

    /// <summary>
    /// Manually updates the current value of a goal owned by the current
    /// user. Only valid for goal types with no computed progress source
    /// (currently GoalType.ChapterCount) — see UpdateGoalProgressCommandHandler.
    /// </summary>
    /// <param name="id">The goal id.</param>
    /// <param name="request">The goal's new current value.</param>
    /// <returns>The updated goal.</returns>
    /// <response code="200">The progress was updated.</response>
    /// <response code="400">The request failed validation, or the goal's progress is calculated automatically.</response>
    /// <response code="404">The goal does not exist or is not owned by the current user.</response>
    [HttpPatch("{id:guid}/progress")]
    public async Task<ActionResult<GoalDto>> UpdateGoalProgress(
        Guid id,
        [FromBody] UpdateGoalProgressRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGoalProgressCommand(id, request.CurrentValue);
        var goal = await _mediator.Send(command, cancellationToken);

        return Ok(goal);
    }

    /// <summary>Deletes (soft-deletes) a goal owned by the current user.</summary>
    /// <param name="id">The goal id.</param>
    /// <response code="204">The goal was deleted.</response>
    /// <response code="404">The goal does not exist or is not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteGoal(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteGoalCommand(id), cancellationToken);

        return NoContent();
    }
}