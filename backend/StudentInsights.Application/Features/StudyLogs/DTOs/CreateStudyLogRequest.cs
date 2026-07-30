// StudentInsights.Application/Features/StudyLogs/DTOs/CreateStudyLogRequest.cs
namespace StudentInsights.Application.Features.StudyLogs.DTOs;

/// <summary>
/// User-supplied fields for creating a StudyLog. UserId is intentionally
/// absent — ownership is derived server-side from the referenced Course
/// (CreateStudyLogCommandHandler resolves and validates CourseId against
/// ICurrentUserService), never trusted from client input.
/// </summary>
public record CreateStudyLogRequest(
    Guid CourseId,
    DateTime StudyDateUtc,
    int DurationMinutes,
    string? Notes);