// StudentInsights.Application/Features/Exams/Commands/DeleteExam/DeleteExamCommand.cs
using MediatR;

namespace StudentInsights.Application.Features.Exams.Commands.DeleteExam;

public record DeleteExamCommand(Guid ExamId) : IRequest;