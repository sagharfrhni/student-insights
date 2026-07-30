using MediatR;

namespace StudentInsights.Application.Features.Courses.Commands.DeleteCourse;

public record DeleteCourseCommand(Guid CourseId) : IRequest;