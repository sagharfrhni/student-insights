using MediatR;
using StudentInsights.Application.Features.Courses.DTOs;

namespace StudentInsights.Application.Features.Courses.Commands.CreateCourse;

/// <summary>
/// UserId is deliberately absent — it is never accepted from client input.
/// CreateCourseCommandHandler resolves it from ICurrentUserService.
/// </summary>
public record CreateCourseCommand(
    string Name,
    int Credits,
    string? InstructorName) : IRequest<CourseDto>;