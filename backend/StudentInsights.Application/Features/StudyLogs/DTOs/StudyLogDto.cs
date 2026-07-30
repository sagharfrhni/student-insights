// StudentInsights.Application/Features/StudyLogs/DTOs/StudyLogDto.cs
namespace StudentInsights.Application.Features.StudyLogs.DTOs;

/// <summary>
/// Read model for a StudyLog, returned from queries. CourseName is a
/// denormalized display field (from StudyLog.Course.Name) so API
/// consumers don't need a second round-trip just to show which course a
/// session belongs to — same pattern as ExamDto.CourseName.
/// </summary>
public record StudyLogDto(
    Guid Id,
    Guid CourseId,
    string CourseName,
    DateTime StudyDateUtc,
    int DurationMinutes,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);