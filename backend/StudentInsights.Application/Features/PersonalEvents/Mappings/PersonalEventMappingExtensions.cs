// StudentInsights.Application/Features/PersonalEvents/Mappings/PersonalEventMappingExtensions.cs
using StudentInsights.Application.Features.PersonalEvents.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.PersonalEvents.Mappings;

/// <summary>
/// Manual PersonalEvent -&gt; PersonalEventDto mapping (no AutoMapper/
/// Mapster), mirroring CourseMappingExtensions/ExamMappingExtensions.
/// Unlike ExamMappingExtensions, this mapping has no in-memory-only
/// members (no value-object flattening, no navigation-derived display
/// field), so the list query's handler is free to project straight into
/// PersonalEventDto via .Select() instead of materializing the entity
/// first. ToDto() is still the mapping used by the single-entity handlers
/// (Create/Update/GetById) where the tracked entity is already in memory
/// after SaveChangesAsync. Create/UpdatePersonalEventRequest are NOT
/// mapped through here: they flow into PersonalEvent.Create(...)/
/// Reschedule(...)/UpdateDetails(...) so the entity's own invariants stay
/// the single source of truth, instead of a mapper writing over
/// private-set properties.
/// </summary>
public static class PersonalEventMappingExtensions
{
    public static PersonalEventDto ToDto(this PersonalEvent personalEvent)
    {
        return new PersonalEventDto(
            personalEvent.Id,
            personalEvent.Title,
            personalEvent.Description,
            personalEvent.StartAtUtc,
            personalEvent.EndAtUtc,
            personalEvent.IsAllDay,
            personalEvent.CreatedAtUtc,
            personalEvent.UpdatedAtUtc);
    }
}