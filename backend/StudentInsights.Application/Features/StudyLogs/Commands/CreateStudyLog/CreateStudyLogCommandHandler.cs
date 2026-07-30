// StudentInsights.Application/Features/StudyLogs/Commands/CreateStudyLog/CreateStudyLogCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.StudyLogs.DTOs;
using StudentInsights.Application.Features.StudyLogs.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.StudyLogs.Commands.CreateStudyLog;

public class CreateStudyLogCommandHandler : IRequestHandler<CreateStudyLogCommand, StudyLogDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateStudyLogCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StudyLogDto> Handle(
        CreateStudyLogCommand request,
        CancellationToken cancellationToken)
    {
        // Do not reveal whether the course exists but belongs to another user.
        // Both cases return the same NotFoundException.
        var course = await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == request.CourseId,
                cancellationToken);

        if (course is null || course.UserId != _currentUserService.UserId)
        {
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");
        }

        var studyLog = StudyLog.Create(
            course,
            request.StudyDateUtc,
            request.DurationMinutes,
            request.Notes);

        _context.StudyLogs.Add(studyLog);

        await _context.SaveChangesAsync(cancellationToken);

        // The Course navigation is already available because the Course
        // entity is tracked by the current DbContext, so no additional
        // query is required before mapping.
        return studyLog.ToDto();
    }
}