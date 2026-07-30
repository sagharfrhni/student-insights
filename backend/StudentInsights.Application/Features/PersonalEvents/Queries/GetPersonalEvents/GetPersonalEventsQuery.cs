// StudentInsights.Application/Features/PersonalEvents/Queries/GetPersonalEvents/GetPersonalEventsQuery.cs
using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.PersonalEvents.DTOs;

namespace StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEvents;

/// <summary>
/// From/To are optional, mirroring GetExamsQuery — this same query shape
/// serves both "all my personal events" and the narrower date-range
/// filters the future Calendar aggregation will need. Unlike
/// GetExamsQuery, there is no CourseId-equivalent filter — PersonalEvent
/// has no FK besides UserId, which is never a query parameter since it's
/// always resolved from ICurrentUserService.
/// </summary>
public record GetPersonalEventsQuery(
    PaginationParams Pagination,
    DateTime? From = null,
    DateTime? To = null) : IRequest<PaginatedResult<PersonalEventDto>>;