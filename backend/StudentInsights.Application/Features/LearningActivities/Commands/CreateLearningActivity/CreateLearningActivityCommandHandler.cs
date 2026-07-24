using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.LearningActivities.DTOs;
using StudentInsights.Application.Features.LearningActivities.Mappings;
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
        // Same 404-for-both-cases reasoning as UpdateCourseCommandHandler:
        // a missing course and a course owned by someone else must be
        // indistinguishable to the caller, or CourseId becomes an
        // enumerable resource (IDOR).
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course is null || course.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");

        var activity = LearningActivity.Create(
            course,
            request.Title,
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