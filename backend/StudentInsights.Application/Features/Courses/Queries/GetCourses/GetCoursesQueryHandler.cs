using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Courses.Queries.GetCourses;

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, PaginatedResult<CourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCoursesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Where(c => c.UserId == _currentUserService.UserId)
            .OrderByDescending(c => c.CreatedAtUtc);

        var pagedCourses = await PaginatedResult<Course>.CreateAsync(
            query,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);

        // Map happens after materialization — Course.ToDto() can't be
        // translated into SQL by EF Core, so paging/filtering/ordering
        // must all happen on IQueryable<Course> first (above), and the
        // DTO projection happens on the already-fetched page only.
        return pagedCourses.Map(course => course.ToDto());
    }
}