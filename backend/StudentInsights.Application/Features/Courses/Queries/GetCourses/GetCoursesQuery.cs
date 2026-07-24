using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Courses.DTOs;

namespace StudentInsights.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery(PaginationParams Pagination) : IRequest<PaginatedResult<CourseDto>>;