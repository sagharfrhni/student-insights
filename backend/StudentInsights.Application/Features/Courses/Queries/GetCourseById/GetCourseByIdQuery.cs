using MediatR;
using StudentInsights.Application.Features.Courses.DTOs;

namespace StudentInsights.Application.Features.Courses.Queries.GetCourseById;

public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto>;