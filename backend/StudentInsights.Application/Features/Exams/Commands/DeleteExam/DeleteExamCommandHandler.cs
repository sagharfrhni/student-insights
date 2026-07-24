// StudentInsights.Application/Features/Exams/Commands/DeleteExam/DeleteExamCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.Exams.Commands.DeleteExam;

public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteExamCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null || exam.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Exam '{request.ExamId}' was not found.");

        exam.Delete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}