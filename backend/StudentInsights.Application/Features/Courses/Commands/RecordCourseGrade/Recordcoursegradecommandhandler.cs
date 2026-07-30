using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Mappings;

namespace StudentInsights.Application.Features.Courses.Commands.RecordCourseGrade;

public class RecordCourseGradeCommandHandler : IRequestHandler<RecordCourseGradeCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RecordCourseGradeCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CourseDto> Handle(RecordCourseGradeCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        // A missing course and a course owned by someone else both return
        // the identical NotFoundException/404 -- same reasoning as
        // UpdateCourseCommandHandler.
        if (course is null || course.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");

        course.SetFinalGrade(request.Grade);

        await _context.SaveChangesAsync(cancellationToken);

        return course.ToDto();
    }
}