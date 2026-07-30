// StudentInsights.WebApi/Controllers/StudyLogsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.StudyLogs.Commands.CreateStudyLog;
using StudentInsights.Application.Features.StudyLogs.Commands.DeleteStudyLog;
using StudentInsights.Application.Features.StudyLogs.Commands.UpdateStudyLog;
using StudentInsights.Application.Features.StudyLogs.DTOs;
using StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogById;
using StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogs;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Manages the authenticated user's study logs. Every endpoint requires
/// authentication; ownership of a given StudyLog (direct via
/// StudyLog.UserId, or via the referenced Course on create) is enforced
/// inside the corresponding Application-layer handler, not here. This
/// controller contains no business logic — it only translates HTTP
/// requests into MediatR commands/queries and MediatR results into HTTP
/// responses.
/// </summary>
[ApiController]
[Route("api/study-logs")]
[Authorize]
public class StudyLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudyLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new study log under a course owned by the current user.</summary>
    /// <param name="request">The study log's course, date, duration, and notes.</param>
    /// <returns>The created study log.</returns>
    /// <response code="201">The study log was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The referenced course does not exist or is not owned by the current user.</response>
    [HttpPost]
    public async Task<ActionResult<StudyLogDto>> CreateStudyLog(
        [FromBody] CreateStudyLogRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateStudyLogCommand(request.CourseId, request.StudyDateUtc, request.DurationMinutes, request.Notes);
        var studyLog = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetStudyLogById), new { id = studyLog.Id }, studyLog);
    }

    /// <summary>Gets a single study log owned by the current user.</summary>
    /// <param name="id">The study log id.</param>
    /// <returns>The requested study log.</returns>
    /// <response code="200">The study log was found.</response>
    /// <response code="404">The study log does not exist or is not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudyLogDto>> GetStudyLogById(Guid id, CancellationToken cancellationToken)
    {
        var studyLog = await _mediator.Send(new GetStudyLogByIdQuery(id), cancellationToken);

        return Ok(studyLog);
    }

    /// <summary>
    /// Gets a paged list of the current user's study logs, most recent
    /// first, optionally filtered by course, semester, and/or a date range.
    /// </summary>
    /// <param name="pagination">Page number and page size.</param>
    /// <param name="courseId">Optional course id to filter by.</param>
    /// <param name="semester">Optional semester to filter by (via the study log's course), e.g. "Fall 2026".</param>
    /// <param name="from">Optional inclusive lower bound on study date (UTC).</param>
    /// <param name="to">Optional inclusive upper bound on study date (UTC).</param>
    /// <returns>A page of study logs.</returns>
    /// <response code="200">The page was retrieved.</response>
    /// <response code="400">'from' is later than 'to'.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<StudyLogDto>>> GetStudyLogs(
        [FromQuery] PaginationParams pagination,
        [FromQuery] Guid? courseId,
        [FromQuery] string? semester,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var studyLogs = await _mediator.Send(new GetStudyLogsQuery(pagination, courseId, semester, from, to), cancellationToken);

        return Ok(studyLogs);
    }

    /// <summary>Updates a study log owned by the current user.</summary>
    /// <param name="id">The study log id.</param>
    /// <param name="request">The study log's new date, duration, and notes.</param>
    /// <returns>The updated study log.</returns>
    /// <response code="200">The study log was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The study log does not exist or is not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudyLogDto>> UpdateStudyLog(
        Guid id,
        [FromBody] UpdateStudyLogRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStudyLogCommand(id, request.StudyDateUtc, request.DurationMinutes, request.Notes);
        var studyLog = await _mediator.Send(command, cancellationToken);

        return Ok(studyLog);
    }

    /// <summary>Deletes (soft-deletes) a study log owned by the current user.</summary>
    /// <param name="id">The study log id.</param>
    /// <response code="204">The study log was deleted.</response>
    /// <response code="404">The study log does not exist or is not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStudyLog(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteStudyLogCommand(id), cancellationToken);

        return NoContent();
    }
}