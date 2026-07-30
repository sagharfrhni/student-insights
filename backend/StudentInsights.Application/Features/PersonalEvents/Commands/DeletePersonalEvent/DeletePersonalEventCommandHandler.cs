// StudentInsights.Application/Features/PersonalEvents/Commands/DeletePersonalEvent/DeletePersonalEventCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.DeletePersonalEvent;

public class DeletePersonalEventCommandHandler : IRequestHandler<DeletePersonalEventCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeletePersonalEventCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeletePersonalEventCommand request, CancellationToken cancellationToken)
    {
        var personalEvent = await _context.PersonalEvents
            .FirstOrDefaultAsync(pe => pe.Id == request.PersonalEventId, cancellationToken);

        if (personalEvent is null || personalEvent.UserId != _currentUserService.UserId)
            throw new NotFoundException($"PersonalEvent '{request.PersonalEventId}' was not found.");

        // Delete() is inherited from BaseEntity and only flips
        // IsDeleted/DeletedAtUtc — ApplicationDbContext.SaveChangesAsync's
        // soft-delete interceptor converts the resulting EntityState.
        // Deleted only if DbSet.Remove() were called instead; here the
        // entity is already Modified via Delete(), so SaveChangesAsync
        // persists it as a normal update, exactly like DeleteExamCommandHandler.
        personalEvent.Delete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}