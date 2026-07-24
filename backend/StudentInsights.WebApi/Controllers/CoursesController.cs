using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Courses.Commands.CreateCourse;
using StudentInsights.Application.Features.Courses.Commands.DeleteCourse;
using StudentInsights.Application.Features.Courses.Commands.UpdateCourse;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Queries.GetCourseById;
using StudentInsights.Application.Features.Courses.Queries.GetCourses;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Manages the authenticated user's courses. Every endpoint requires
/// authentication; ownership of a given Course is enforced inside the
/// corresponding Application-layer handler, not here. This controller
/// contains no business logic — it only translates HTTP requests into
/// MediatR commands/queries and MediatR results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new course for the current user.</summary>
    /// <param name="request">The course's name, credits, and instructor.</param>
    /// <returns>The created course.</returns>
    /// <response code="201">The course was created.</response>
    /// <response code="400">The request failed validation.</response>
    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourse(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCourseCommand(request.Name, request.Credits, request.InstructorName);
        var course = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
    }

    /// <summary>Gets a single course owned by the current user.</summary>
    /// <param name="id">The course id.</param>
    /// <returns>The requested course.</returns>
    /// <response code="200">The course was found.</response>
    /// <response code="404">The course does not exist or is not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetCourseById(Guid id, CancellationToken cancellationToken)
    {
        var course = await _mediator.Send(new GetCourseByIdQuery(id), cancellationToken);

        return Ok(course);
    }

    /// <summary>Gets a paged list of the current user's courses, newest first.</summary>
    /// <param name="pagination">Page number and page size.</param>
    /// <returns>A page of courses.</returns>
    /// <response code="200">The page was retrieved.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CourseDto>>> GetCourses(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var courses = await _mediator.Send(new GetCoursesQuery(pagination), cancellationToken);

        return Ok(courses);
    }

    /// <summary>Updates a course owned by the current user.</summary>
    /// <param name="id">The course id.</param>
    /// <param name="request">The course's new name, credits, and instructor.</param>
    /// <returns>The updated course.</returns>
    /// <response code="200">The course was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">The course does not exist or is not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourseDto>> UpdateCourse(
        Guid id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseCommand(id, request.Name, request.Credits, request.InstructorName);
        var course = await _mediator.Send(command, cancellationToken);

        return Ok(course);
    }

    /// <summary>Deletes (soft-deletes) a course owned by the current user.</summary>
    /// <param name="id">The course id.</param>
    /// <response code="204">The course was deleted.</response>
    /// <response code="404">The course does not exist or is not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCourseCommand(id), cancellationToken);

        return NoContent();
    }
}