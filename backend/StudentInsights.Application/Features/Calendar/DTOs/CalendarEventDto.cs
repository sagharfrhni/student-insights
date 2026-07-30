using StudentInsights.Application.Features.Calendar.Enums;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Calendar.DTOs;

/// <summary>
/// Uniform read model returned by the Calendar module, regardless of
/// which of the five sources (ClassSchedule, Exam, LearningActivity,
/// Goal, PersonalEvent) an event originated from. Deliberately reuses
/// <see cref="ActivityPriority"/> and <see cref="ActivityStatus"/> from
/// Domain.Enums instead of duplicating them, the same way ExamDto reuses
/// the Grade-derived decimal? shape rather than inventing a parallel one.
///
/// StartAtUtc/EndAtUtc follow the project's existing *Utc DateTime
/// convention (see ExamDto.ExamDateUtc, LearningActivityDto.DueDateUtc,
/// PersonalEvent.StartAtUtc/EndAtUtc) rather than DateTimeOffset — this
/// project has no DateTimeOffset usage anywhere, and DateTime already
/// round-trips correctly via the registered UtcDateTimeConverter.
/// </summary>
/// <param name="Type">Discriminator the client uses for icon/color and to interpret Priority/Status.</param>
/// <param name="SourceId">Id of the originating entity (ClassSchedule/Exam/LearningActivity/Goal/PersonalEvent), so the client can deep-link back to it.</param>
/// <param name="Title">Human-readable label, already composed server-side.</param>
/// <param name="StartAtUtc">Single point in time, or the start of a time range for classes and personal events.</param>
/// <param name="EndAtUtc">Null for point events (Exam, Deadline, Goal); populated for Class and Personal events.</param>
/// <param name="RelatedCourseId">The owning course, where applicable. Null for Goal and Personal events.</param>
/// <param name="Priority">Populated only for Deadline (from LearningActivity.Priority); null otherwise.</param>
/// <param name="Status">Populated only for Deadline (from LearningActivity.Status); null otherwise.</param>
public record CalendarEventDto(
    CalendarEventType Type,
    Guid SourceId,
    string Title,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    Guid? RelatedCourseId,
    ActivityPriority? Priority,
    ActivityStatus? Status);