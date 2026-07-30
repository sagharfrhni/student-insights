using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Mappings;
using StudentInsights.Domain.Common;

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

        if (course is null || course.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");

        var trimmedName = request.Name.Trim();

        // Same duplicate-name rule as CreateCourseCommandHandler (scoped to
        // the course's own semester -- Semester is immutable after creation,
        // so there's no request value to compare against here), excluding
        // the course being renamed itself so re-saving with an unchanged
        // name is not rejected as a duplicate of itself.
        var isDuplicate = await _context.Courses
            .AnyAsync(c => c.Id != course.Id && c.UserId == course.UserId
                && c.Name == trimmedName && c.Semester == course.Semester, cancellationToken);

        if (isDuplicate)
            throw new DomainException("A course with this name already exists for this semester.");

        course.Rename(trimmedName);
        course.UpdateCredits(request.Credits);
        course.UpdateInstructor(request.InstructorName);

        await _context.SaveChangesAsync(cancellationToken);

        return course.ToDto();
    }
}