// StudentInsights.Application/Features/StudyLogs/Commands/UpdateStudyLog/UpdateStudyLogCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.StudyLogs.DTOs;
using StudentInsights.Application.Features.StudyLogs.Mappings;

namespace StudentInsights.Application.Features.StudyLogs.Commands.UpdateStudyLog;

public class UpdateStudyLogCommandHandler : IRequestHandler<UpdateStudyLogCommand, StudyLogDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateStudyLogCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StudyLogDto> Handle(
        UpdateStudyLogCommand request,
        CancellationToken cancellationToken)
    {
        // Include(Course) is required here — ToDto()'s CourseName needs
        // Course loaded and happens in-memory (see
        // StudyLogMappingExtensions), same as UpdateExamCommandHandler.
        var studyLog = await _context.StudyLogs
            .Include(sl => sl.Course)
            .FirstOrDefaultAsync(
                sl => sl.Id == request.StudyLogId,
                cancellationToken);

        if (studyLog is null || studyLog.UserId != _currentUserService.UserId)
            throw new NotFoundException($"StudyLog '{request.StudyLogId}' was not found.");

        studyLog.Reschedule(request.StudyDateUtc);
        studyLog.UpdateDuration(request.DurationMinutes);
        studyLog.UpdateNotes(request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return studyLog.ToDto();
    }
}