// StudentInsights.Application/Features/PersonalEvents/Queries/GetPersonalEventById/GetPersonalEventByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.PersonalEvents.DTOs;
using StudentInsights.Application.Features.PersonalEvents.Mappings;

namespace StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEventById;

public class GetPersonalEventByIdQueryHandler : IRequestHandler<GetPersonalEventByIdQuery, PersonalEventDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPersonalEventByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PersonalEventDto> Handle(GetPersonalEventByIdQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: pure read, never mutated/saved. No Include() is
        // needed, unlike GetExamByIdQueryHandler's Include(Course) —
        // PersonalEventDto has no navigation-derived field, so ToDto()
        // works directly off the loaded entity.
        var personalEvent = await _context.PersonalEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(pe => pe.Id == request.PersonalEventId, cancellationToken);

        if (personalEvent is null || personalEvent.UserId != _currentUserService.UserId)
            throw new NotFoundException($"PersonalEvent '{request.PersonalEventId}' was not found.");

        return personalEvent.ToDto();
    }
}