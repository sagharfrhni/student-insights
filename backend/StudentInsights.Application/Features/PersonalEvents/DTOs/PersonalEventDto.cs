// StudentInsights.Application/Features/PersonalEvents/DTOs/PersonalEventDto.cs
namespace StudentInsights.Application.Features.PersonalEvents.DTOs;

/// <summary>
/// Read model for a PersonalEvent, returned from queries and commands.
/// Unlike ExamDto, there is no denormalized parent-name field to flatten —
/// PersonalEvent has no navigation property besides the owning User, which
/// is never exposed to the client (ownership is implicit: every endpoint
/// only ever returns events belonging to the current user).
/// </summary>
public record PersonalEventDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool IsAllDay,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);