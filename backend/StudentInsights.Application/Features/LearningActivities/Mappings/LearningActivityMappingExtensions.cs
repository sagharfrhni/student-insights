using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.LearningActivities.Mappings;

/// <summary>
/// Manual LearningActivity -&gt; LearningActivityDto mapping (no
/// AutoMapper/Mapster), same reasoning as CourseMappingExtensions: a
/// single explicit method is simpler than configuring/maintaining a
/// mapping profile for a shape this small.
///
/// ToDto takes courseName as an explicit parameter rather than reading
/// activity.Course.Name off the navigation property. This keeps the
/// mapping usable from two different call shapes without forcing either
/// one to load data it doesn't need:
/// - Command handlers (Create/Update/Delete/UpdateStatus) already have the
///   owning Course loaded for the ownership check, so they pass its Name
///   straight through with no extra query.
/// - GetLearningActivitiesQueryHandler projects CourseName directly out of
///   a joined IQueryable via .Select() (per the roadmap's no-N+1
///   guidance), so it never materializes a LearningActivity with its
///   Course navigation loaded in the first place.
///
/// CreateLearningActivityRequest/UpdateLearningActivityRequest are NOT
/// mapped through here, for the same reason CreateCourseRequest/
/// UpdateCourseRequest aren't: they flow into
/// LearningActivity.Create(...)/UpdateDetails(...)/Reschedule(...)/
/// SetPriority(...) so the entity's own invariants stay the single source
/// of truth instead of a mapper writing over private-set properties.
/// </summary>
public static class LearningActivityMappingExtensions
{
    public static LearningActivityDto ToDto(this LearningActivity activity, string courseName)
    {
        return new LearningActivityDto(
            activity.Id,
            activity.CourseId,
            courseName,
            activity.Title,
            activity.Type,
            activity.DueDateUtc,
            activity.Priority,
            activity.Status,
            activity.Description,
            activity.ResourceLink,
            activity.CompletedAtUtc,
            activity.CreatedAtUtc,
            activity.UpdatedAtUtc);
    }
}