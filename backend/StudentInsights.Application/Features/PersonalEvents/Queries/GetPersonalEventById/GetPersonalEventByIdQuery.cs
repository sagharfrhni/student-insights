// StudentInsights.Application/Features/PersonalEvents/Queries/GetPersonalEventById/GetPersonalEventByIdQuery.cs
using MediatR;
using StudentInsights.Application.Features.PersonalEvents.DTOs;

namespace StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEventById;

public record GetPersonalEventByIdQuery(Guid PersonalEventId) : IRequest<PersonalEventDto>;