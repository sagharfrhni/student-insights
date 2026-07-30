// StudentInsights.Application/Features/Exams/Commands/RecordExamGrade/RecordExamGradeCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Mappings;

namespace StudentInsights.Application.Features.Exams.Commands.RecordExamGrade;

public class RecordExamGradeCommandHandler : IRequestHandler<RecordExamGradeCommand, ExamDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RecordExamGradeCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ExamDto> Handle(RecordExamGradeCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        // A missing exam and an exam owned by someone else both return the
        // identical NotFoundException/404 -- same reasoning as
        // UpdateExamCommandHandler.
        if (exam is null || exam.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Exam '{request.ExamId}' was not found.");

        exam.RecordGrade(request.Grade);

        await _context.SaveChangesAsync(cancellationToken);

        // The Course navigation is already loaded via .Include above, so
        // ToDto() (which needs exam.Course.Name) requires no extra query --
        // same reasoning as UpdateExamCommandHandler.
        return exam.ToDto();
    }
}