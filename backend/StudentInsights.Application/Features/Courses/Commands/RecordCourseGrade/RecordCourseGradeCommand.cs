using MediatR;
using StudentInsights.Application.Features.Courses.DTOs;

namespace StudentInsights.Application.Features.Courses.Commands.RecordCourseGrade;

/// <summary>
/// Records (or overwrites) a Course's FinalGrade. Only the grade is
/// accepted -- everything else about the course is edited through
/// UpdateCourseCommand instead, same separation UpdateGoalProgressCommand
/// uses versus UpdateGoalCommand.
/// </summary>
public record RecordCourseGradeCommand(Guid CourseId, decimal Grade) : IRequest<CourseDto>;