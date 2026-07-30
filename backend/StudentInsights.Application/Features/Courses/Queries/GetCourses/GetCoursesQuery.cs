using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Courses.DTOs;

namespace StudentInsights.Application.Features.Courses.Queries.GetCourses;

/// <summary>
/// Semester is optional so this same query shape serves both "all my
/// courses" and a single term's course list -- same "optional filter,
/// narrower shape" convention as GetExamsQuery's CourseId/From/To.
/// </summary>
public record GetCoursesQuery(
    PaginationParams Pagination,
    string? Semester = null) : IRequest<PaginatedResult<CourseDto>>;