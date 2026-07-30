// StudentInsights.Application/Features/StudyLogs/Queries/GetStudyLogById/GetStudyLogByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.StudyLogs.DTOs;
using StudentInsights.Application.Features.StudyLogs.Mappings;

namespace StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogById;

public class GetStudyLogByIdQueryHandler : IRequestHandler<GetStudyLogByIdQuery, StudyLogDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStudyLogByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StudyLogDto> Handle(GetStudyLogByIdQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: pure read, never mutated/saved. Include(Course) is
        // required here — StudyLogDto.CourseName in ToDto() needs Course
        // loaded and happens in-memory (see StudyLogMappingExtensions).
        var studyLog = await _context.StudyLogs
            .AsNoTracking()
            .Include(sl => sl.Course)
            .FirstOrDefaultAsync(sl => sl.Id == request.StudyLogId, cancellationToken);

        // Same 404-for-both-cases reasoning as GetExamByIdQueryHandler:
        // don't let a 403 confirm that a StudyLogId belonging to someone
        // else exists. StudyLog.UserId is checked directly (no join
        // through Course needed) since it's set from Course.UserId at
        // creation time.
        if (studyLog is null || studyLog.UserId != _currentUserService.UserId)
            throw new NotFoundException($"StudyLog '{request.StudyLogId}' was not found.");

        return studyLog.ToDto();
    }
}