using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Mappings;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Analytics.Queries.GetAssignmentProgress;

public class GetAssignmentProgressQueryHandler
    : IRequestHandler<GetAssignmentProgressQuery, AssignmentProgressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAssignmentProgressQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AssignmentProgressDto> Handle(
        GetAssignmentProgressQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LearningActivities
            .AsNoTracking()
            .Where(la => la.UserId == _currentUserService.UserId);

        if (request.From.HasValue)
            query = query.Where(la => la.DueDateUtc >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(la => la.DueDateUtc <= request.To.Value);

        // Grouped COUNT translated into a single SQL query (Completed vs.
        // everything else) -- same pattern as
        // GetDashboardSummaryQueryHandler.GetActiveActivityCountsAsync,
        // never materializes the underlying rows just to count them.
        var statusCounts = await query
            .GroupBy(la => la.Status == ActivityStatus.Completed)
            .Select(g => new { IsCompleted = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var completed = statusCounts.FirstOrDefault(x => x.IsCompleted)?.Count ?? 0;
        var pending = statusCounts.FirstOrDefault(x => !x.IsCompleted)?.Count ?? 0;

        return AnalyticsMappingExtensions.ToAssignmentProgressDto(completed, pending);
    }
}