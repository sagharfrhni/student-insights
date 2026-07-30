using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Time;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Mappings;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Analytics.Queries.GetWeeklyActivity;

public class GetWeeklyActivityQueryHandler : IRequestHandler<GetWeeklyActivityQuery, WeeklyActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetWeeklyActivityQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WeeklyActivityDto> Handle(GetWeeklyActivityQuery request, CancellationToken cancellationToken)
    {
        // WeekBoundary is the single source of truth for "when does a week
        // start" -- reused here exactly as Dashboard reuses it for
        // WeeklyStudyMinutes, so the two never disagree about what "this
        // week" means. A supplied WeekStartDate is floored to its actual
        // week start, not used verbatim, so passing any day of the target
        // week yields the same result.
        var weekStartUtc = WeekBoundary.GetUtcWeekStart(request.WeekStartDate ?? DateTime.UtcNow);
        var weekEndUtc = weekStartUtc.AddDays(7);

        // Bounded to exactly one week, so fetching the raw completion
        // timestamps and bucketing them in memory (see
        // AnalyticsMappingExtensions.ToWeeklyActivityDto) is cheap and
        // avoids relying on EF Core's translation of a per-day GroupBy
        // expression -- the same in-memory bucketing approach Study Time
        // uses for all of its granularities (see
        // AnalyticsMappingExtensions.ToStudyTimeDto).
        //
        // CompletedAtUtc!.Value is safe here: LearningActivity.Complete()
        // is the only place Status becomes Completed, and it always sets
        // CompletedAtUtc in the same call -- see LearningActivity.cs.
        var completedAtTimestamps = await _context.LearningActivities
            .AsNoTracking()
            .Where(la =>
                la.UserId == _currentUserService.UserId &&
                la.Status == ActivityStatus.Completed &&
                la.CompletedAtUtc >= weekStartUtc &&
                la.CompletedAtUtc < weekEndUtc)
            .Select(la => la.CompletedAtUtc!.Value)
            .ToListAsync(cancellationToken);

        return AnalyticsMappingExtensions.ToWeeklyActivityDto(weekStartUtc, completedAtTimestamps);
    }
}