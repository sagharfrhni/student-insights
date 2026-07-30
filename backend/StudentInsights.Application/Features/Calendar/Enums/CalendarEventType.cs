namespace StudentInsights.Application.Features.Calendar.Enums;

/// <summary>
/// Discriminator for the source of a <see cref="DTOs.CalendarEventDto"/>.
/// Calendar has no entity of its own (see the module's architectural
/// classification: it is a read-side aggregation over five existing
/// sources), so this enum exists purely to let API consumers tell the
/// five source shapes apart once they've been merged into one list.
/// </summary>
public enum CalendarEventType
{
    /// <summary>A recurring weekly class session, expanded from a Course's ClassSchedule.</summary>
    Class = 0,

    /// <summary>A one-off exam date.</summary>
    Exam = 1,

    /// <summary>A LearningActivity due date (assignment or project).</summary>
    Deadline = 2,

    /// <summary>A Goal with a concrete target date.</summary>
    Goal = 3,

    /// <summary>An ad-hoc PersonalEvent.</summary>
    Personal = 4
}