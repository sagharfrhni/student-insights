// StudentInsights.Application/Features/PersonalEvents/Commands/UpdatePersonalEvent/UpdatePersonalEventCommand.cs
using MediatR;
using StudentInsights.Application.Features.PersonalEvents.DTOs;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.UpdatePersonalEvent;

/// <summary>
/// IsAllDay is deliberately absent — PersonalEvent.cs exposes no mutator
/// for it (only Create() sets it), so it is immutable after creation and
/// never part of the editable payload, the same way UpdateExamCommand
/// omits CourseId because an exam cannot change parent course after
/// creation.
/// </summary>
public record UpdatePersonalEventCommand(
    Guid PersonalEventId,
    string Title,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? Description) : IRequest<PersonalEventDto>;