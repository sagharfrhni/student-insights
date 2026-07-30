// StudentInsights.Application/Features/StudyLogs/Commands/DeleteStudyLog/DeleteStudyLogCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.StudyLogs.Commands.DeleteStudyLog;

public class DeleteStudyLogCommandHandler : IRequestHandler<DeleteStudyLogCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteStudyLogCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteStudyLogCommand request, CancellationToken cancellationToken)
    {
        var studyLog = await _context.StudyLogs
            .FirstOrDefaultAsync(sl => sl.Id == request.StudyLogId, cancellationToken);

        if (studyLog is null || studyLog.UserId != _currentUserService.UserId)
            throw new NotFoundException($"StudyLog '{request.StudyLogId}' was not found.");

        studyLog.Delete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}