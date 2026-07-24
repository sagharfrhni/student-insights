using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Courses.Mappings;

/// <summary>
/// Manual Course -&gt; CourseDto mapping (no AutoMapper/Mapster). Kept
/// explicit because FinalGrade is the Grade value object (implicit
/// conversion to decimal) — a mapping library would still need a custom
/// resolver for that one field, so a library buys no reduction in
/// complexity here, only an extra dependency and reflection-based
/// indirection. CreateCourseRequest/UpdateCourseRequest are NOT mapped
/// through here: they flow into Course.Create(...)/Course.Rename(...)/etc.
/// so the aggregate's own invariants (validation, MarkModified) stay the
/// single source of truth, instead of a mapper writing over private-set
/// properties.
/// </summary>
public static class CourseMappingExtensions
{
    public static CourseDto ToDto(this Course course)
    {
        return new CourseDto(
            course.Id,
            course.Name,
            course.Credits,
            course.InstructorName,
            course.FinalGrade,
            course.CreatedAtUtc,
            course.UpdatedAtUtc);
    }
}