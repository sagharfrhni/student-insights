using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Mappings;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");

        var trimmedName = request.Name.Trim();
        var trimmedSemester = request.Semester.Trim();

        // A student's course list must not contain two entries with the same
        // name IN THE SAME SEMESTER (see CourseConfiguration's matching
        // unique filtered index, which backstops this against a concurrent
        // duplicate request) — the same name legitimately repeating in a
        // different semester (retaking a course, a recurring seminar) is not
        // a duplicate. Relies on SQL Server's default case-insensitive
        // collation for the comparison, same assumption
        // CreateExamCommandHandler's Title check already makes.
        var isDuplicate = await _context.Courses
            .AnyAsync(c => c.UserId == user.Id && c.Name == trimmedName && c.Semester == trimmedSemester, cancellationToken);

        if (isDuplicate)
            throw new DomainException("A course with this name already exists for this semester.");

        var course = Course.Create(user, trimmedName, request.Credits, trimmedSemester, request.InstructorName);

        _context.Courses.Add(course);

        await _context.SaveChangesAsync(cancellationToken);

        return course.ToDto();
    }
}