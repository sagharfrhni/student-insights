// StudentInsights.WebApi/Controllers/ExamsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Exams.Commands.CreateExam;
using StudentInsights.Application.Features.Exams.Commands.DeleteExam;
using StudentInsights.Application.Features.Exams.Commands.UpdateExam;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Queries.GetExamById;
using StudentInsights.Application.Features.Exams.Queries.GetExams;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Manages the authenticated user's exams. Every endpoint requires
/// authentication; ownership of a given Exam (direct via Exam.UserId, or
/// via the referenced Course on create) is enforced inside the
/// corresponding Application-layer handler, not here. This controller
/// contains no business logic — it only translates HTTP requests into
/// MediatR commands/queries and MediatR results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/exams")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExamsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new exam under a course owned by the current user.</summary>
    /// <param name="request">The exam's course, title, date, and description.</param>
    /// <returns>The created exam.</returns>
    /// <response code="201">The exam was created.</response>
    /// <response code="400">The request failed validation, or duplicates an existing exam.</response>
    /// <response code="404">The referenced course does not exist or is not owned by the current user.</response>
    [HttpPost]
    public async Task<ActionResult<ExamDto>> CreateExam(
        [FromBody] CreateExamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExamCommand(request.CourseId, request.Title, request.ExamDateUtc, request.Description);
        var exam = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetExamById), new { id = exam.Id }, exam);
    }

    /// <summary>Gets a single exam owned by the current user.</summary>
    /// <param name="id">The exam id.</param>
    /// <returns>The requested exam.</returns>
    /// <response code="200">The exam was found.</response>
    /// <response code="404">The exam does not exist or is not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamDto>> GetExamById(Guid id, CancellationToken cancellationToken)
    {
        var exam = await _mediator.Send(new GetExamByIdQuery(id), cancellationToken);

        return Ok(exam);
    }

    /// <summary>
    /// Gets a paged list of the current user's exams, soonest first,
    /// optionally filtered by course and/or a date range.
    /// </summary>
    /// <param name="pagination">Page number and page size.</param>
    /// <param name="courseId">Optional course id to filter by.</param>
    /// <param name="from">Optional inclusive lower bound on exam date (UTC).</param>
    /// <param name="to">Optional inclusive upper bound on exam date (UTC).</param>
    /// <returns>A page of exams.</returns>
    /// <response code="200">The page was retrieved.</response>
    /// <response code="400">'from' is later than 'to'.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ExamDto>>> GetExams(
        [FromQuery] PaginationParams pagination,
        [FromQuery] Guid? courseId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var exams = await _mediator.Send(new GetExamsQuery(pagination, courseId, from, to), cancellationToken);

        return Ok(exams);
    }

    /// <summary>Updates an exam owned by the current user.</summary>
    /// <param name="id">The exam id.</param>
    /// <param name="request">The exam's new title, date, and description.</param>
    /// <returns>The updated exam.</returns>
    /// <response code="200">The exam was updated.</response>
    /// <response code="400">The request failed validation, or duplicates an existing exam.</response>
    /// <response code="404">The exam does not exist or is not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExamDto>> UpdateExam(
        Guid id,
        [FromBody] UpdateExamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamCommand(id, request.Title, request.ExamDateUtc, request.Description);
        var exam = await _mediator.Send(command, cancellationToken);

        return Ok(exam);
    }

    /// <summary>Deletes (soft-deletes) an exam owned by the current user.</summary>
    /// <param name="id">The exam id.</param>
    /// <response code="204">The exam was deleted.</response>
    /// <response code="404">The exam does not exist or is not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExam(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteExamCommand(id), cancellationToken);

        return NoContent();
    }
}