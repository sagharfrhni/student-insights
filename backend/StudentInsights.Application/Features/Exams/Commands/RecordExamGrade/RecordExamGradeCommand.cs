// StudentInsights.Application/Features/Exams/Commands/RecordExamGrade/RecordExamGradeCommand.cs
using MediatR;
using StudentInsights.Application.Features.Exams.DTOs;

namespace StudentInsights.Application.Features.Exams.Commands.RecordExamGrade;

/// <summary>
/// Records (or overwrites) an Exam's Grade. Only the grade is accepted --
/// everything else about the exam is edited through UpdateExamCommand
/// instead, same separation UpdateGoalProgressCommand uses versus
/// UpdateGoalCommand.
/// </summary>
public record RecordExamGradeCommand(Guid ExamId, decimal Grade) : IRequest<ExamDto>;