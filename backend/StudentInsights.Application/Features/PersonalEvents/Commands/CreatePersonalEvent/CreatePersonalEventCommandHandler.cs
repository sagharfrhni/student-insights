// StudentInsights.Application/Features/PersonalEvents/Commands/CreatePersonalEvent/CreatePersonalEventCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.PersonalEvents.DTOs;
using StudentInsights.Application.Features.PersonalEvents.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.CreatePersonalEvent;

public class CreatePersonalEventCommandHandler : IRequestHandler<CreatePersonalEventCommand, PersonalEventDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePersonalEventCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PersonalEventDto> Handle(
        CreatePersonalEventCommand request,
        CancellationToken cancellationToken)
    {
        // PersonalEvent.Create() takes the owning User entity, not a bare
        // Guid, so the current user must be loaded first — the same
        // pattern CreateExamCommandHandler uses to load the referenced
        // Course before calling Exam.Create(). ICurrentUserService.UserId
        // is resolved from a validated JWT behind [Authorize], so a
        // missing row here would only ever indicate a deleted/corrupted
        // account rather than an ownership issue — still surfaced as 404,
        // consistent with how every other handler in the project reports
        // "the referenced row isn't there."
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");
        }

        var personalEvent = PersonalEvent.Create(
            user,
            request.Title,
            request.StartAtUtc,
            request.EndAtUtc,
            request.IsAllDay,
            request.Description);

        _context.PersonalEvents.Add(personalEvent);

        await _context.SaveChangesAsync(cancellationToken);

        return personalEvent.ToDto();
    }
}