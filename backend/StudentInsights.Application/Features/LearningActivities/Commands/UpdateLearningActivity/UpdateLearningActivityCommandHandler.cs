using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Application.Features.LearningActivities.Mappings;

namespace StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivity;

public class UpdateLearningActivityCommandHandler : IRequestHandler<UpdateLearningActivityCommand, LearningActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLearningActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LearningActivityDto> Handle(UpdateLearningActivityCommand request, CancellationToken cancellationToken)
    {
        // Course is included (a single-entity join, not a list query) purely
        // to supply CourseName for the returned DTO — see the reasoning on
        // LearningActivityMappingExtensions.ToDto.
        var activity = await _context.LearningActivities
            .Include(la => la.Course)
            .FirstOrDefaultAsync(la => la.Id == request.LearningActivityId, cancellationToken);

        // Same 404-for-both-cases reasoning as UpdateCourseCommandHandler.
        if (activity is null || activity.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Learning activity '{request.LearningActivityId}' was not found.");

        activity.UpdateDetails(request.Title, request.Description, request.ResourceLink);
        activity.Reschedule(request.DueDateUtc);
        activity.SetPriority(request.Priority);

        await _context.SaveChangesAsync(cancellationToken);

        return activity.ToDto(activity.Course.Name);
    }
}