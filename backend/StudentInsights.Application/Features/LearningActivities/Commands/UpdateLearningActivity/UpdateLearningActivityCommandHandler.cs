using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Application.Features.LearningActivities.Mappings;
using StudentInsights.Domain.Common;

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
        var activity = await _context.LearningActivities
            .Include(la => la.Course)
            .FirstOrDefaultAsync(la => la.Id == request.LearningActivityId, cancellationToken);

        if (activity is null || activity.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Learning activity '{request.LearningActivityId}' was not found.");

        var trimmedTitle = request.Title.Trim();

        // Same same-course/same-title/same-minute duplicate rule as
        // CreateLearningActivityCommandHandler, excluding the activity being
        // edited itself so re-saving with unchanged values is not rejected
        // as a duplicate of itself.
        var dueDateMinute = new DateTime(
            request.DueDateUtc.Year,
            request.DueDateUtc.Month,
            request.DueDateUtc.Day,
            request.DueDateUtc.Hour,
            request.DueDateUtc.Minute,
            0,
            request.DueDateUtc.Kind);

        var nextMinute = dueDateMinute.AddMinutes(1);

        var isDuplicate = await _context.LearningActivities.AnyAsync(
            la =>
                la.Id != activity.Id &&
                la.CourseId == activity.CourseId &&
                la.Title == trimmedTitle &&
                la.DueDateUtc >= dueDateMinute &&
                la.DueDateUtc < nextMinute,
            cancellationToken);

        if (isDuplicate)
        {
            throw new DomainException(
                "A learning activity with the same title and due date already exists for this course.");
        }

        activity.UpdateDetails(trimmedTitle, request.Description, request.ResourceLink);
        activity.Reschedule(request.DueDateUtc);
        activity.SetPriority(request.Priority);

        await _context.SaveChangesAsync(cancellationToken);

        return activity.ToDto(activity.Course.Name);
    }
}