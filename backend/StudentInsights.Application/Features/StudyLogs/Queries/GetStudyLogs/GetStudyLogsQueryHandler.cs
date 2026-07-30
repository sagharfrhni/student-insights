// StudentInsights.Application/Features/StudyLogs/Queries/GetStudyLogs/GetStudyLogsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.StudyLogs.DTOs;
using StudentInsights.Application.Features.StudyLogs.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogs;

public class GetStudyLogsQueryHandler : IRequestHandler<GetStudyLogsQuery, PaginatedResult<StudyLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStudyLogsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<StudyLogDto>> Handle(GetStudyLogsQuery request, CancellationToken cancellationToken)
    {
        // Include(Course) here is a single reference navigation (many-to-
        // one), so combined with Skip/Take it translates to one JOIN per
        // page — not the collection-Include over-fetch risk the roadmap
        // warned about. A raw .Select() projection was deliberately not
        // used instead: mapping happens after materialization via ToDto()
        // (see StudyLogMappingExtensions), exactly like GetExamsQueryHandler.
        var query = _context.StudyLogs
            .AsNoTracking()
            .Include(sl => sl.Course)
            .Where(sl => sl.UserId == _currentUserService.UserId);

        if (request.CourseId.HasValue)
            query = query.Where(sl => sl.CourseId == request.CourseId.Value);

        if (!string.IsNullOrWhiteSpace(request.Semester))
            query = query.Where(sl => sl.Course.Semester == request.Semester);

        if (request.From.HasValue)
            query = query.Where(sl => sl.StudyDateUtc >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(sl => sl.StudyDateUtc <= request.To.Value);

        // Descending by StudyDateUtc (most recent session first), unlike
        // GetExamsQueryHandler's ascending "soonest first" order — a study
        // log is a retrospective record, so "what did I just log" is the
        // useful default here, the same way GetCoursesQueryHandler defaults
        // to CreatedAtUtc descending for its own log-like list.
        query = query.OrderByDescending(sl => sl.StudyDateUtc);

        var pagedStudyLogs = await PaginatedResult<StudyLog>.CreateAsync(
            query,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);

        return pagedStudyLogs.Map(studyLog => studyLog.ToDto());
    }
}