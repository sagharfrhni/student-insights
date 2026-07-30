// StudentInsights.Application/Features/Exams/Queries/GetExamById/GetExamByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Mappings;

namespace StudentInsights.Application.Features.Exams.Queries.GetExamById;

public class GetExamByIdQueryHandler : IRequestHandler<GetExamByIdQuery, ExamDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExamByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ExamDto> Handle(GetExamByIdQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: pure read, never mutated/saved. Include(Course) is
        // required here — ExamDto.CourseName and the Grade -> decimal?
        // flattening in ToDto() both need Course loaded and happen
        // in-memory (see ExamMappingExtensions).
        var exam = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        // Same 404-for-both-cases reasoning as GetCourseByIdQueryHandler:
        // don't let a 403 confirm that an ExamId belonging to someone else
        // exists. Exam.UserId is checked directly (no join through Course
        // needed) since it's set from Course.UserId at creation time.
        if (exam is null || exam.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Exam '{request.ExamId}' was not found.");

        return exam.ToDto();
    }
}