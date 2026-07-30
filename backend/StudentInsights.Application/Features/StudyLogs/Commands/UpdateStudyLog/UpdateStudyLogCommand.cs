// StudentInsights.Application/Features/StudyLogs/Commands/UpdateStudyLog/UpdateStudyLogCommand.cs
using MediatR;
using StudentInsights.Application.Features.StudyLogs.DTOs;

namespace StudentInsights.Application.Features.StudyLogs.Commands.UpdateStudyLog;

/// <summary>
/// CourseId is deliberately absent — a study log cannot be moved to a
/// different course after creation (see UpdateStudyLogCommandHandler for
/// the reasoning), so it is never part of the editable payload, mirroring
/// UpdateExamCommand. StudyDateUtc/DurationMinutes/Notes are all supplied
/// together as a full replace, per project convention for this module.
/// </summary>
public record UpdateStudyLogCommand(
    Guid StudyLogId,
    DateTime StudyDateUtc,
    int DurationMinutes,
    string? Notes) : IRequest<StudyLogDto>;