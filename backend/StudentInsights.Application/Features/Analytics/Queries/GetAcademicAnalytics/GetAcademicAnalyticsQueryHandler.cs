using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Academics;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Mappings;

namespace StudentInsights.Application.Features.Analytics.Queries.GetAcademicAnalytics;

public class GetAcademicAnalyticsQueryHandler : IRequestHandler<GetAcademicAnalyticsQuery, AcademicAnalyticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAcademicAnalyticsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AcademicAnalyticsDto> Handle(GetAcademicAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Same fetch shape GetDashboardSummaryQueryHandler already uses to
        // compute its own GPA: a term's courses are few enough that
        // loading the full entity list (no Include, so only Course's own
        // columns are touched) is simpler than a narrower projection, for
        // no real cost -- this is the same table, the same small-per-user
        // scale, and the same "GPA input" purpose Dashboard already reads
        // it for.
        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.UserId == _currentUserService.UserId)
            .ToListAsync(cancellationToken);

        // Scoped to the user's current semester (see
        // CurrentSemesterCourseProvider's remarks for what "current"
        // means) rather than every course the user has ever entered --
        // same scoping GoalProgressInputsProvider and
        // GetDashboardSummaryQueryHandler already apply to GPA, so this
        // screen's GPA, average grade, graded/ungraded counts, and
        // per-course chart can never silently disagree with the numbers
        // shown on Goals or the Dashboard.
        var currentSemesterCourses = CurrentSemesterCourseProvider.GetCurrentSemesterCourses(courses).ToList();

        return AnalyticsMappingExtensions.ToAcademicAnalyticsDto(currentSemesterCourses);
    }
}
