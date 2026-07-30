using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Mappings;

namespace StudentInsights.Application.Features.Analytics.Queries.GetStudyTime;

public class GetStudyTimeQueryHandler : IRequestHandler<GetStudyTimeQuery, StudyTimeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStudyTimeQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StudyTimeDto> Handle(GetStudyTimeQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudyLogs
            .AsNoTracking()
            .Where(sl => sl.UserId == _currentUserService.UserId);

        if (request.From.HasValue)
            query = query.Where(sl => sl.StudyDateUtc >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(sl => sl.StudyDateUtc <= request.To.Value);

        // Narrow projection -- only the two columns bucketing/summing
        // actually needs, no Include(Course) -- same discipline
        // GetDashboardSummaryQueryHandler.GetStudyLogsAsync already
        // follows for this exact table.
        var studyLogs = await query
            .Select(sl => new ValueTuple<DateTime, int>(sl.StudyDateUtc, sl.DurationMinutes))
            .ToListAsync(cancellationToken);

        // Granularity is validated (non-null, defined) by
        // GetStudyTimeQueryValidator before Handle ever runs, so the
        // null-forgiving unwrap here is safe, not a guess.
        return AnalyticsMappingExtensions.ToStudyTimeDto(request.Granularity!.Value, studyLogs);
    }
}