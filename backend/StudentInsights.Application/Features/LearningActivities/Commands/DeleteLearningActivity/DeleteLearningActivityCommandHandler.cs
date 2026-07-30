using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.LearningActivities.Commands.DeleteLearningActivity;

public class DeleteLearningActivityCommandHandler : IRequestHandler<DeleteLearningActivityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLearningActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteLearningActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _context.LearningActivities
            .FirstOrDefaultAsync(la => la.Id == request.LearningActivityId, cancellationToken);

        if (activity is null || activity.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Learning activity '{request.LearningActivityId}' was not found.");

        // Goal.RelatedActivityId's FK is configured with OnDelete(SetNull),
        // but that only fires on a real SQL DELETE -- every delete in this
        // project is a soft delete (an UPDATE via BaseEntity.Delete()), so
        // the FK cascade never actually runs. Without this, a ProjectDeadline
        // goal tracking this activity would be left pointing at a now-
        // invisible activity forever, permanently stuck at "not yet
        // available" progress.
        var relatedGoals = await _context.Goals
            .Where(g => g.RelatedActivityId == activity.Id)
            .ToListAsync(cancellationToken);

        foreach (var relatedGoal in relatedGoals)
            relatedGoal.Delete();

        activity.Delete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}