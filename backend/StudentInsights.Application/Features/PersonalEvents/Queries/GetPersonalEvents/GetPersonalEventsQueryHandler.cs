// StudentInsights.Application/Features/PersonalEvents/Queries/GetPersonalEvents/GetPersonalEventsQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.PersonalEvents.DTOs;

namespace StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEvents;

public class GetPersonalEventsQueryHandler : IRequestHandler<GetPersonalEventsQuery, PaginatedResult<PersonalEventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPersonalEventsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<PersonalEventDto>> Handle(
        GetPersonalEventsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PersonalEvents
            .AsNoTracking()
            .Where(pe => pe.UserId == _currentUserService.UserId);

        if (request.From.HasValue)
            query = query.Where(pe => pe.StartAtUtc >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(pe => pe.StartAtUtc <= request.To.Value);

        // Ascending by StartAtUtc (soonest first), matching GetExamsQuery's
        // "what's coming up next" default ordering — the natural order for
        // a calendar-style read model.
        query = query.OrderBy(pe => pe.StartAtUtc);

        // Projected directly to PersonalEventDto via .Select() rather than
        // materializing PersonalEvent and mapping via ToDto() afterwards.
        // This differs from GetExamsQueryHandler, which materializes first
        // because ExamDto.Grade requires an in-memory-only value-object
        // conversion. PersonalEvent has no such member, so the projection
        // below translates cleanly to SQL and avoids fetching columns the
        // DTO doesn't need (see PersonalEventMappingExtensions' doc comment
        // and Section 14 of the roadmap).
        var projectedQuery = query.Select(pe => new PersonalEventDto(
            pe.Id,
            pe.Title,
            pe.Description,
            pe.StartAtUtc,
            pe.EndAtUtc,
            pe.IsAllDay,
            pe.CreatedAtUtc,
            pe.UpdatedAtUtc));

        return await PaginatedResult<PersonalEventDto>.CreateAsync(
            projectedQuery,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);
    }
}