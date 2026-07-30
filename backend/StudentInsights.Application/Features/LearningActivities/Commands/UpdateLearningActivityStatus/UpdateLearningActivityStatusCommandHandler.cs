using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Application.Features.LearningActivities.Mappings;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivityStatus;

/// <summary>
/// Drives LearningActivity's existing state-changing methods
/// (Start/Complete/Reopen) instead of writing Status directly — the
/// property has a private setter, so this is also the only way the
/// Handler *can* change it. See ApplyTransition for how each of the six
/// real transitions maps onto those three methods.
/// </summary>
public class UpdateLearningActivityStatusCommandHandler : IRequestHandler<UpdateLearningActivityStatusCommand, LearningActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLearningActivityStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LearningActivityDto> Handle(UpdateLearningActivityStatusCommand request, CancellationToken cancellationToken)
    {
        var activity = await _context.LearningActivities
            .Include(la => la.Course)
            .FirstOrDefaultAsync(la => la.Id == request.LearningActivityId, cancellationToken);

        if (activity is null || activity.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Learning activity '{request.LearningActivityId}' was not found.");

        ApplyTransition(activity, request.NewStatus);

        await _context.SaveChangesAsync(cancellationToken);

        return activity.ToDto(activity.Course.Name);
    }

    /// <summary>
    /// Every transition between the three statuses is allowed; a request
    /// for the activity's current status is a no-op success (idempotent
    /// PATCH) rather than an error. Start() is the only guarded entity
    /// method (it throws if already Completed), so Completed -&gt; InProgress
    /// goes through Reopen() first to clear that guard before Start().
    /// </summary>
    private static void ApplyTransition(LearningActivity activity, ActivityStatus newStatus)
    {
        if (activity.Status == newStatus)
            return;

        switch (newStatus)
        {
            case ActivityStatus.NotStarted:
                activity.Reopen();
                break;
            case ActivityStatus.InProgress:
                if (activity.Status == ActivityStatus.Completed)
                    activity.Reopen();
                activity.Start();
                break;
            case ActivityStatus.Completed:
                activity.Complete();
                break;
        }
    }
}