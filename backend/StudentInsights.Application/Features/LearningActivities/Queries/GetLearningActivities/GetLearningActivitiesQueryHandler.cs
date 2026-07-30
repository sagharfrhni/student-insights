using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.LearningActivities.DTOs;

namespace StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivities;

public class GetLearningActivitiesQueryHandler : IRequestHandler<GetLearningActivitiesQuery, PaginatedResult<LearningActivityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLearningActivitiesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<LearningActivityDto>> Handle(GetLearningActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LearningActivities
            .AsNoTracking()
            .Where(la => la.UserId == _currentUserService.UserId);

        if (request.CourseId is not null)
            query = query.Where(la => la.CourseId == request.CourseId);

        if (request.Status is not null)
            query = query.Where(la => la.Status == request.Status);

        if (request.Type is not null)
            query = query.Where(la => la.Type == request.Type);

        // Unlike GetCoursesQueryHandler, mapping happens *inside* the
        // IQueryable pipeline instead of after materialization: this
        // projection only touches entity properties and a single
        // navigation property (la.Course.Name), which EF Core can
        // translate into SQL — it's the LearningActivityDto constructor
        // call itself, not a call to the ToDto() extension method, that
        // makes the translation possible. That lets
        // PaginatedResult<T>.CreateAsync count, sort, page, and select in
        // one round trip, with no N+1 and no full entity/graph
        // materialization. (Course.ToDto() can't be translated this way,
        // which is why the Course module maps after fetching a page of
        // entities instead — see GetCoursesQueryHandler.)
        var projectedQuery = query
            .OrderBy(la => la.DueDateUtc)
            .Select(la => new LearningActivityDto(
                la.Id,
                la.CourseId,
                la.Course.Name,
                la.Title,
                la.Type,
                la.DueDateUtc,
                la.Priority,
                la.Status,
                la.Description,
                la.ResourceLink,
                la.CompletedAtUtc,
                la.CreatedAtUtc,
                la.UpdatedAtUtc));

        return await PaginatedResult<LearningActivityDto>.CreateAsync(
            projectedQuery,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);
    }
}