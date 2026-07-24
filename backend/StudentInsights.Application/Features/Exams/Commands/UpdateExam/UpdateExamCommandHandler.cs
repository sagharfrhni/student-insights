// StudentInsights.Application/Features/Exams/Commands/UpdateExam/UpdateExamCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Mappings;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Exams.Commands.UpdateExam;

public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand, ExamDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateExamCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ExamDto> Handle(
        UpdateExamCommand request,
        CancellationToken cancellationToken)
    {
        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(
                e => e.Id == request.ExamId,
                cancellationToken);

        if (exam is null || exam.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Exam '{request.ExamId}' was not found.");

        var trimmedTitle = request.Title.Trim();

        // Compare dates with minute precision.
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
                e.Id != exam.Id &&
                e.CourseId == exam.CourseId &&
                e.Title == trimmedTitle &&
                e.ExamDateUtc >= examMinute &&
                e.ExamDateUtc < nextMinute,
            cancellationToken);

        if (isDuplicate)
        {
            throw new DomainException(
                "An exam with the same title and date already exists for this course.");
        }

        exam.Rename(trimmedTitle);
        exam.Reschedule(request.ExamDateUtc);
        exam.UpdateDescription(request.Description);

        await _context.SaveChangesAsync(cancellationToken);

        return exam.ToDto();
    }
}