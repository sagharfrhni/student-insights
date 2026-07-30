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

        activity.Delete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}