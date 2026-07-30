using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.LearningActivities.DTOs;

namespace StudentInsights.Application.Features.LearningActivities.Queries.GetLearningActivityById;

public class GetLearningActivityByIdQueryHandler : IRequestHandler<GetLearningActivityByIdQuery, LearningActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLearningActivityByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LearningActivityDto> Handle(GetLearningActivityByIdQuery request, CancellationToken cancellationToken)
    {
        // Ownership is filtered directly in the query rather than checked
        // after loading (contrast GetCourseByIdQueryHandler): the DTO
        // below is a direct Select projection with no UserId left on it to
        // check afterward, so folding "belongs to the current user" into
        // the WHERE clause is simpler and cheaper while staying a single
        // query. A missing activity and someone else's activity are still
        // indistinguishable 404s either way (same anti-IDOR reasoning as
        // UpdateCourseCommandHandler).
        //
        // Referencing la.Course.Name inside Select (rather than
        // .Include(la => la.Course)) lets EF Core translate this into a
        // single SQL join that pulls only the columns the DTO needs,
        // instead of materializing the full LearningActivity + Course
        // entity graph.
        var activity = await _context.LearningActivities
            .AsNoTracking()
            .Where(la => la.Id == request.LearningActivityId && la.UserId == _currentUserService.UserId)
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
                la.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (activity is null)
            throw new NotFoundException($"Learning activity '{request.LearningActivityId}' was not found.");

        return activity;
    }
}