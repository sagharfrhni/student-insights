// StudentInsights.Application/Features/StudyLogs/Queries/GetStudyLogs/GetStudyLogsQuery.cs
using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.StudyLogs.DTOs;

namespace StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogs;

/// <summary>
/// CourseId/Semester/From/To are all optional so this same query shape can
/// serve both "all my study logs" and the narrower filters the Goals/Analytics
/// features read through IApplicationDbContext directly (this query is
/// only for client/list consumption). A CourseId that doesn't belong to
/// the current user simply yields an empty page (the base filter below
/// already scopes to StudyLog.UserId) — not a 403/404, same reasoning as
/// GetExamsQuery. Semester filters via the referenced Course's own
/// Semester (StudyLog has no Semester of its own), same reasoning and
/// placement as GetExamsQuery's CourseId/Semester.
/// </summary>
public record GetStudyLogsQuery(
    PaginationParams Pagination,
    Guid? CourseId = null,
    string? Semester = null,
    DateTime? From = null,
    DateTime? To = null) : IRequest<PaginatedResult<StudyLogDto>>;