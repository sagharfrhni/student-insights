using MediatR;
using StudentInsights.Application.Features.Courses.DTOs;

namespace StudentInsights.Application.Features.Courses.Commands.UpdateCourse;

public record UpdateCourseCommand(
    Guid CourseId,
    string Name,
    int Credits,
    string? InstructorName) : IRequest<CourseDto>;