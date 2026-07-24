// StudentInsights.Application/Features/Exams/Mappings/ExamMappingExtensions.cs
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Exams.Mappings;

/// <summary>
/// Manual Exam -&gt; ExamDto mapping (no AutoMapper/Mapster), mirroring
/// CourseMappingExtensions. Must run against an already-materialized Exam
/// with Course loaded (via .Include(e => e.Course)) — CourseName and the
/// Grade -&gt; decimal? flattening both rely on in-memory access, since
/// Grade's implicit conversion operator cannot be translated into SQL by
/// EF Core inside a LINQ-to-Entities projection. CreateExamRequest/
/// UpdateExamRequest are NOT mapped through here: they flow into
/// Exam.Create(...)/Exam.Rename(...)/etc. so the entity's own invariants
/// stay the single source of truth, instead of a mapper writing over
/// private-set properties.
/// </summary>
public static class ExamMappingExtensions
{
    public static ExamDto ToDto(this Exam exam)
    {
        return new ExamDto(
            exam.Id,
            exam.CourseId,
            exam.Course.Name,
            exam.Title,
            exam.ExamDateUtc,
            exam.Description,
            exam.Grade,
            exam.CreatedAtUtc,
            exam.UpdatedAtUtc);
    }
}