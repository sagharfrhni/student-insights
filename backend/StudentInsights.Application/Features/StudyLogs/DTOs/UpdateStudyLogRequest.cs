// StudentInsights.Application/Features/StudyLogs/DTOs/UpdateStudyLogRequest.cs
namespace StudentInsights.Application.Features.StudyLogs.DTOs;

/// <summary>
/// User-supplied fields for updating a StudyLog. CourseId is deliberately
/// absent — a study log cannot be moved to a different course after
/// creation (see UpdateExamRequest for the same convention on Exam); the
/// log's Id being updated comes from the route/command, not this payload.
/// </summary>
public record UpdateStudyLogRequest(
    DateTime StudyDateUtc,
    int DurationMinutes,
    string? Notes);