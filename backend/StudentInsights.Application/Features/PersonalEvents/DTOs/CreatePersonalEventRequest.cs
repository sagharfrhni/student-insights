// StudentInsights.Application/Features/PersonalEvents/DTOs/CreatePersonalEventRequest.cs
namespace StudentInsights.Application.Features.PersonalEvents.DTOs;

/// <summary>
/// User-supplied fields for creating a PersonalEvent. UserId is
/// intentionally absent — ownership is always resolved server-side from
/// ICurrentUserService (see CreatePersonalEventCommandHandler), never
/// trusted from client input, the same guard every other Create*Request
/// in the project follows.
/// EndAtUtc is required, not optional — PersonalEvent.Create() has no
/// overload that omits it, and Domain throws if EndAtUtc &lt;= StartAtUtc,
/// so the request mirrors the entity rather than the roadmap doc's
/// (outdated, against actual code) assumption of a nullable EndAt.
/// </summary>
public record CreatePersonalEventRequest(
    string Title,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool IsAllDay,
    string? Description);