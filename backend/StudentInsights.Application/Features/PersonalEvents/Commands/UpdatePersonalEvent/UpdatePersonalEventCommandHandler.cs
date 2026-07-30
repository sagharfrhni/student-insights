// StudentInsights.Application/Features/PersonalEvents/Commands/UpdatePersonalEvent/UpdatePersonalEventCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.PersonalEvents.DTOs;
using StudentInsights.Application.Features.PersonalEvents.Mappings;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.UpdatePersonalEvent;

public class UpdatePersonalEventCommandHandler : IRequestHandler<UpdatePersonalEventCommand, PersonalEventDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePersonalEventCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PersonalEventDto> Handle(
        UpdatePersonalEventCommand request,
        CancellationToken cancellationToken)
    {
        // No Include() is needed here, unlike UpdateExamCommandHandler's
        // Include(e => e.Course) — PersonalEventDto has no navigation-
        // derived field, so the tracked entity alone is enough to map
        // after SaveChangesAsync.
        var personalEvent = await _context.PersonalEvents
            .FirstOrDefaultAsync(pe => pe.Id == request.PersonalEventId, cancellationToken);

        if (personalEvent is null || personalEvent.UserId != _currentUserService.UserId)
            throw new NotFoundException($"PersonalEvent '{request.PersonalEventId}' was not found.");

        // PersonalEvent exposes two independent mutators rather than one
        // generic Update() — UpdateDetails() owns the Title/Description
        // invariant (Title required), Reschedule() owns the
        // EndAtUtc > StartAtUtc invariant. Both throw DomainException
        // (mapped to 400 by ExceptionHandlingMiddleware) if violated,
        // as a defense-in-depth backstop behind the validator.
        personalEvent.UpdateDetails(request.Title, request.Description);
        personalEvent.Reschedule(request.StartAtUtc, request.EndAtUtc);

        await _context.SaveChangesAsync(cancellationToken);

        return personalEvent.ToDto();
    }
}