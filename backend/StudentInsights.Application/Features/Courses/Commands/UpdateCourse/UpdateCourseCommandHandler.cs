using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Mappings;

namespace StudentInsights.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CourseDto> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        // A missing course and a course owned by someone else both return
        // the identical NotFoundException/404. A 403 here would confirm
        // "this CourseId exists" to a caller who doesn't own it, turning
        // CourseId into an enumerable resource (IDOR). 404 for both keeps
        // another user's data existence unconfirmable.
        if (course is null || course.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");

        course.Rename(request.Name);
        course.UpdateCredits(request.Credits);
        course.UpdateInstructor(request.InstructorName);

        await _context.SaveChangesAsync(cancellationToken);

        return course.ToDto();
    }
}