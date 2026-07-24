using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Courses.DTOs;
using StudentInsights.Application.Features.Courses.Mappings;
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
        // Course.Create takes the full User entity, not a Guid, by
        // established project convention (see RefreshToken.Create in
        // LoginCommandHandler). Loading it here also double-checks the
        // user hasn't been soft-deleted/deactivated since the JWT was
        // issued — defense in depth, not just satisfying the signature.
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");

        var course = Course.Create(user, request.Name, request.Credits, request.InstructorName);

        _context.Courses.Add(course);

        await _context.SaveChangesAsync(cancellationToken);

        return course.ToDto();
    }
}