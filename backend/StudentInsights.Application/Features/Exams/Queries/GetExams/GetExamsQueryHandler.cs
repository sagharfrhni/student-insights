// StudentInsights.Application/Features/Exams/Queries/GetExams/GetExamsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Exams.Queries.GetExams;

public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, PaginatedResult<ExamDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExamsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<ExamDto>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
    {
        // Include(Course) here is a single reference navigation (many-to-
        // one), so combined with Skip/Take it translates to one JOIN per
        // page — not the collection-Include over-fetch risk the roadmap
        // warned about. A raw .Select() projection was deliberately not
        // used instead: Grade's value-object -> decimal? conversion can't
        // be translated into SQL, only run in-memory via ToDto() (see
        // ExamMappingExtensions), so mapping has to happen after
        // materialization, exactly like GetCoursesQueryHandler.
        var query = _context.Exams
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.UserId == _currentUserService.UserId);

        if (request.CourseId.HasValue)
            query = query.Where(e => e.CourseId == request.CourseId.Value);

        if (request.From.HasValue)
            query = query.Where(e => e.ExamDateUtc >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.ExamDateUtc <= request.To.Value);

        // Ascending by ExamDateUtc (soonest first), not CreatedAtUtc
        // descending like GetCoursesQueryHandler — exams are calendar
        // events, so "what's coming up next" is the useful default order,
        // and it's what the future Calendar/Dashboard consumers of this
        // query will actually want.
        query = query.OrderBy(e => e.ExamDateUtc);

        var pagedExams = await PaginatedResult<Exam>.CreateAsync(
            query,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);

        return pagedExams.Map(exam => exam.ToDto());
    }
}