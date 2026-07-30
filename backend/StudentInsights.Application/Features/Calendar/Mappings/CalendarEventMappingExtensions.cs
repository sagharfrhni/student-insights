using System;
using System.Collections.Generic;
using StudentInsights.Application.Features.Calendar.DTOs;
using StudentInsights.Application.Features.Calendar.Enums;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Calendar.Mappings;

/// <summary>
/// Manual entity -&gt; CalendarEventDto mapping (no AutoMapper/Mapster),
/// mirroring the project's existing ExamMappingExtensions/
/// LearningActivityMappingExtensions convention. All five methods assume
/// an already-materialized entity — the same precondition
/// ExamMappingExtensions.ToDto() has for its Course navigation — rather
/// than being written as translatable Select() expressions, because
/// ToCalendarEvents(ClassSchedule) below inherently cannot be: expanding
/// a weekly recurrence into concrete dated occurrences is a C# loop, not
/// something EF Core can turn into SQL. Since one source must be mapped
/// in-memory regardless, every source here follows the same, single
/// mapping style for consistency, instead of mixing an in-query
/// projection for four sources with a special-cased in-memory path for
/// the fifth. Exactly which fields each query fetches before calling
/// these methods is decided in GetCalendarEventsQueryHandler.
/// </summary>
public static class CalendarEventMappingExtensions
{
    /// <summary>
    /// Exam -&gt; CalendarEventDto. Does not require Exam.Course to be
    /// loaded: RelatedCourseId comes directly off Exam.CourseId, and per
    /// the roadmap's own Title examples, an exam's calendar title is just
    /// its own Title, with no course name prefix.
    /// </summary>
    public static CalendarEventDto ToCalendarEvent(this Exam exam)
    {
        return new CalendarEventDto(
            CalendarEventType.Exam,
            exam.Id,
            exam.Title,
            exam.ExamDateUtc,
            null,
            exam.CourseId,
            null,
            null);
    }

    /// <summary>
    /// LearningActivity -&gt; CalendarEventDto. Priority/Status are the
    /// only case where CalendarEventDto populates those two fields — they
    /// are null for every other source, per Section 5's field table.
    /// </summary>
    public static CalendarEventDto ToCalendarEvent(this LearningActivity activity)
    {
        return new CalendarEventDto(
            CalendarEventType.Deadline,
            activity.Id,
            $"{activity.Title} due",
            activity.DueDateUtc,
            null,
            activity.CourseId,
            activity.Priority,
            activity.Status);
    }

    /// <summary>
    /// Goal -&gt; CalendarEventDto. Precondition: goal.TargetDateUtc must
    /// be non-null. Calendar only ever includes dated goals (Section 2's
    /// "Goal without a date belongs on the Dashboard, not the calendar"
    /// rule) — the query handler is responsible for filtering to
    /// TargetDateUtc != null before this is ever called, the same way
    /// CalendarEventMappingExtensions.ToCalendarEvent(Exam) trusts its
    /// caller to have scoped rows to the current user. Goal has no Title
    /// property, so one is composed from its Type. RelatedCourseId is
    /// null: Goal never references a Course, only an optional
    /// LearningActivity (via RelatedActivityId) for ProjectDeadline goals.
    /// </summary>
    public static CalendarEventDto ToCalendarEvent(this Goal goal)
    {
        return new CalendarEventDto(
            CalendarEventType.Goal,
            goal.Id,
            $"Goal: {goal.Type}",
            goal.TargetDateUtc!.Value,
            null,
            null,
            null,
            null);
    }

    /// <summary>
    /// PersonalEvent -&gt; CalendarEventDto. Unlike Exam/LearningActivity/
    /// Goal, PersonalEvent is a genuine range (StartAtUtc/EndAtUtc, both
    /// required on the entity, per PersonalEvent.Create's
    /// "end must be after start" invariant) rather than a single point in
    /// time — the roadmap's Section 3 assumption of a single EventDate
    /// column does not match the actual entity shape.
    /// </summary>
    public static CalendarEventDto ToCalendarEvent(this PersonalEvent personalEvent)
    {
        return new CalendarEventDto(
            CalendarEventType.Personal,
            personalEvent.Id,
            personalEvent.Title,
            personalEvent.StartAtUtc,
            personalEvent.EndAtUtc,
            null,
            null,
            null);
    }

    /// <summary>
    /// ClassSchedule -&gt; zero or more CalendarEventDto occurrences, one
    /// per matching day-of-week that falls within [fromUtc, toUtc]. Takes
    /// courseId/courseName explicitly rather than reading
    /// classSchedule.Course, mirroring
    /// LearningActivityMappingExtensions.ToDto(activity, courseName) —
    /// ClassSchedule has no UserId of its own, and requiring every caller
    /// to have the Course navigation loaded would risk exactly the kind
    /// of over-fetch Section 11 warns against.
    ///
    /// Boundary handling: fromUtc/toUtc may carry a time-of-day component
    /// (e.g. "the next 7 days from right now"), so each candidate
    /// occurrence's own start time is checked against the full range
    /// rather than just its calendar date — otherwise a class earlier in
    /// the day than "now" on the very first matching weekday would be
    /// incorrectly included.
    /// </summary>
    public static IEnumerable<CalendarEventDto> ToCalendarEvents(
        this ClassSchedule classSchedule,
        Guid courseId,
        string courseName,
        DateTime fromUtc,
        DateTime toUtc)
    {
        if (toUtc < fromUtc)
            yield break;

        var currentDay = fromUtc.Date;
        var daysUntilFirstMatch = ((int)classSchedule.DayOfWeek - (int)currentDay.DayOfWeek + 7) % 7;
        currentDay = currentDay.AddDays(daysUntilFirstMatch);

        while (currentDay <= toUtc.Date)
        {
            var dayStartUtc = DateTime.SpecifyKind(currentDay, DateTimeKind.Utc);
            var startAtUtc = dayStartUtc.Add(classSchedule.StartTime);
            var endAtUtc = dayStartUtc.Add(classSchedule.EndTime);

            if (startAtUtc >= fromUtc && startAtUtc <= toUtc)
            {
                yield return new CalendarEventDto(
                    CalendarEventType.Class,
                    classSchedule.Id,
                    $"{courseName} — Class",
                    startAtUtc,
                    endAtUtc,
                    courseId,
                    null,
                    null);
            }

            currentDay = currentDay.AddDays(7);
        }
    }
}