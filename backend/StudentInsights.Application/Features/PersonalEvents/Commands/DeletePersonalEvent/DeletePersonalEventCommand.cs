// StudentInsights.Application/Features/PersonalEvents/Commands/DeletePersonalEvent/DeletePersonalEventCommand.cs
using MediatR;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.DeletePersonalEvent;

public record DeletePersonalEventCommand(Guid PersonalEventId) : IRequest;