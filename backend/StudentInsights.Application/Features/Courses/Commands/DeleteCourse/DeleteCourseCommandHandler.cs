using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.Courses.Commands.DeleteCourse;

public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }


    public async Task Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course is null || course.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");

        var learningActivities = await _context.LearningActivities
            .Where(la => la.CourseId == course.Id)
            .ToListAsync(cancellationToken);

        var exams = await _context.Exams
            .Where(e => e.CourseId == course.Id)
            .ToListAsync(cancellationToken);

        var studyLogs = await _context.StudyLogs
            .Where(sl => sl.CourseId == course.Id)
            .ToListAsync(cancellationToken);

        foreach (var learningActivity in learningActivities)
            learningActivity.Delete();

        foreach (var exam in exams)
            exam.Delete();

        foreach (var studyLog in studyLogs)
            studyLog.Delete();

        course.Delete();

        await _context.SaveChangesAsync(cancellationToken);
    }

}