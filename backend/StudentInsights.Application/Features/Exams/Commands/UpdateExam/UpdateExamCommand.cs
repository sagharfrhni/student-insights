// StudentInsights.Application/Features/Exams/Commands/UpdateExam/UpdateExamCommand.cs
using MediatR;
using StudentInsights.Application.Features.Exams.DTOs;

namespace StudentInsights.Application.Features.Exams.Commands.UpdateExam;

/// <summary>
/// CourseId is deliberately absent — an exam cannot be moved to a
/// different course after creation (see UpdateExamCommandHandler for the
/// reasoning), so it is never part of the editable payload.
/// </summary>
public record UpdateExamCommand(
    Guid ExamId,
    string Title,
    DateTime ExamDateUtc,
    string? Description) : IRequest<ExamDto>;