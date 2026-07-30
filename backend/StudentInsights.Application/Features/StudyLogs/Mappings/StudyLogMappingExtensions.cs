// StudentInsights.Application/Features/StudyLogs/Mappings/StudyLogMappingExtensions.cs
using StudentInsights.Application.Features.StudyLogs.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.StudyLogs.Mappings;

/// <summary>
/// Manual StudyLog -&gt; StudyLogDto mapping (no AutoMapper/Mapster),
/// mirroring ExamMappingExtensions. Must run against an already-
/// materialized StudyLog with Course loaded (via .Include(sl => sl.Course)),
/// since CourseName is read in-memory. CreateStudyLogRequest/
/// UpdateStudyLogRequest are NOT mapped through here: they flow into
/// StudyLog.Create(...)/.Reschedule(...)/.UpdateDuration(...)/
/// .UpdateNotes(...) so the entity's own invariants stay the single
/// source of truth, instead of a mapper writing over private-set
/// properties.
/// </summary>
public static class StudyLogMappingExtensions
{
    public static StudyLogDto ToDto(this StudyLog studyLog)
    {
        return new StudyLogDto(
            studyLog.Id,
            studyLog.CourseId,
            studyLog.Course.Name,
            studyLog.StudyDateUtc,
            studyLog.DurationMinutes,
            studyLog.Notes,
            studyLog.CreatedAtUtc,
            studyLog.UpdatedAtUtc);
    }
}