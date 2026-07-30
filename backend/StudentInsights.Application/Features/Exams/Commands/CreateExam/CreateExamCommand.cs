// StudentInsights.Application/Features/Exams/Commands/CreateExam/CreateExamCommand.cs
using MediatR;
using StudentInsights.Application.Features.Exams.DTOs;

namespace StudentInsights.Application.Features.Exams.Commands.CreateExam;

/// <summary>
/// UserId is deliberately absent — it is never accepted from client
/// input. CreateExamCommandHandler resolves ownership by loading the
/// referenced Course and checking Course.UserId against
/// ICurrentUserService, the same way CreateCourseCommandHandler resolves
/// the current User.
/// </summary>
public record CreateExamCommand(
    Guid CourseId,
    string Title,
    DateTime ExamDateUtc,
    string? Description) : IRequest<ExamDto>;