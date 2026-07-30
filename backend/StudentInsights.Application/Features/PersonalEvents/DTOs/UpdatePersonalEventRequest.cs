// StudentInsights.Application/Features/PersonalEvents/DTOs/UpdatePersonalEventRequest.cs
namespace StudentInsights.Application.Features.PersonalEvents.DTOs;

/// <summary>
/// User-supplied fields for updating a PersonalEvent. IsAllDay is
/// deliberately absent — PersonalEvent.cs exposes no mutator for it (only
/// Create() sets it), so it is immutable after creation and never part of
/// the editable payload, by design. UserId is absent for the same
/// ownership reason as CreatePersonalEventRequest. The event's Id being
/// updated comes from the route/command, not from this payload, matching
/// UpdateExamRequest's convention.
/// </summary>
public record UpdatePersonalEventRequest(
    string Title,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? Description);