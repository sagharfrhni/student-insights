// StudentInsights.Application/Features/StudyLogs/Commands/CreateStudyLog/CreateStudyLogCommand.cs
using MediatR;
using StudentInsights.Application.Features.StudyLogs.DTOs;

namespace StudentInsights.Application.Features.StudyLogs.Commands.CreateStudyLog;

/// <summary>
/// UserId is deliberately absent — it is never accepted from client
/// input. CreateStudyLogCommandHandler resolves ownership by loading the
/// referenced Course and checking Course.UserId against
/// ICurrentUserService, the same way CreateExamCommandHandler resolves
/// the current User.
/// </summary>
public record CreateStudyLogCommand(
    Guid CourseId,
    DateTime StudyDateUtc,
    int DurationMinutes,
    string? Notes) : IRequest<StudyLogDto>;