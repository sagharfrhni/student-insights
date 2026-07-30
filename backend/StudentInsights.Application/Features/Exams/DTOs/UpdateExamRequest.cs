// StudentInsights.Application/Features/Exams/DTOs/UpdateExamRequest.cs
namespace StudentInsights.Application.Features.Exams.DTOs;

/// <summary>
/// User-supplied fields for updating an Exam. CourseId is deliberately
/// absent — an exam cannot be moved to a different course after creation
/// (see UpdateExamCommandHandler); the exam's Id being updated comes from
/// the route/command, not from this payload.
/// </summary>
public record UpdateExamRequest(
    string Title,
    DateTime ExamDateUtc,
    string? Description);