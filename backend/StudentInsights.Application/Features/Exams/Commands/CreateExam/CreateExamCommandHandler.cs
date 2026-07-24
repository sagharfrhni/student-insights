// StudentInsights.Application/Features/Exams/Commands/CreateExam/CreateExamCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Mappings;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Exams.Commands.CreateExam;

public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, ExamDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateExamCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ExamDto> Handle(
        CreateExamCommand request,
        CancellationToken cancellationToken)
    {
        // Do not reveal whether the course exists but belongs to another user.
        // Both cases return the same NotFoundException.
        var course = await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == request.CourseId,
                cancellationToken);

        if (course is null || course.UserId != _currentUserService.UserId)
        {
            throw new NotFoundException($"Course '{request.CourseId}' was not found.");
        }

        var trimmedTitle = request.Title.Trim();

        // Compare exam dates with minute precision.
        // Seconds and milliseconds are ignored.
        var examMinute = new DateTime(
            request.ExamDateUtc.Year,
            request.ExamDateUtc.Month,
            request.ExamDateUtc.Day,
            request.ExamDateUtc.Hour,
            request.ExamDateUtc.Minute,
            0,
            request.ExamDateUtc.Kind);

        var nextMinute = examMinute.AddMinutes(1);

        var isDuplicate = await _context.Exams.AnyAsync(
            e =>
                e.CourseId == request.CourseId &&
                e.Title == trimmedTitle &&
                e.ExamDateUtc >= examMinute &&
                e.ExamDateUtc < nextMinute,
            cancellationToken);

        if (isDuplicate)
        {
            throw new DomainException(
                "An exam with the same title and date already exists for this course.");
        }

        var exam = Exam.Create(
            course,
            trimmedTitle,
            request.ExamDateUtc,
            request.Description);

        _context.Exams.Add(exam);

        await _context.SaveChangesAsync(cancellationToken);

        // The Course navigation is already available because the Course
        // entity is tracked by the current DbContext, so no additional
        // query is required before mapping.
        return exam.ToDto();
    }
}