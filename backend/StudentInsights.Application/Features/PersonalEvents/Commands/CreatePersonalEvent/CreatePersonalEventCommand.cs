// StudentInsights.Application/Features/PersonalEvents/Commands/CreatePersonalEvent/CreatePersonalEventCommand.cs
using MediatR;
using StudentInsights.Application.Features.PersonalEvents.DTOs;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.CreatePersonalEvent;

/// <summary>
/// UserId is deliberately absent — it is never accepted from client
/// input. CreatePersonalEventCommandHandler resolves the owning User via
/// ICurrentUserService, the same way CreateExamCommandHandler resolves
/// ownership from the referenced Course.
/// </summary>
public record CreatePersonalEventCommand(
    string Title,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool IsAllDay,
    string? Description) : IRequest<PersonalEventDto>;