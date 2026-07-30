using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Application.Features.LearningActivities.Mappings;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.LearningActivities.Commands.CreateLearningActivity;

public class CreateLearningActivityCommandHandler : IRequestHandler<CreateLearningActivityCommand, LearningActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateLearningActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LearningActivityDto> Handle(CreateLearningActivityCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course is null || course.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");

        var trimmedTitle = request.Title.Trim();

        // Compare due dates with minute precision, ignoring seconds/ms
        // noise — same convention and same reasoning as
        // CreateExamCommandHandler's duplicate check. A student re-entering
        // (or double-submitting) the same assignment for the same course
        // and the same due date is almost certainly a mistake, not a new
        // activity. Backstopped by LearningActivityConfiguration's matching
        // unique filtered index.
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
                la.CourseId == request.CourseId &&
                la.Title == trimmedTitle &&
                la.DueDateUtc >= dueDateMinute &&
                la.DueDateUtc < nextMinute,
            cancellationToken);

        if (isDuplicate)
        {
            throw new DomainException(
                "A learning activity with the same title and due date already exists for this course.");
        }

        var activity = LearningActivity.Create(
            course,
            trimmedTitle,
            request.Type,
            request.DueDateUtc,
            request.Priority,
            request.Description,
            request.ResourceLink);

        _context.LearningActivities.Add(activity);

        await _context.SaveChangesAsync(cancellationToken);

        return activity.ToDto(course.Name);
    }
}