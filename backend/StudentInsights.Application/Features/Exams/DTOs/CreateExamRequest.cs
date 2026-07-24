// StudentInsights.Application/Features/Exams/DTOs/CreateExamRequest.cs
namespace StudentInsights.Application.Features.Exams.DTOs;

/// <summary>
/// User-supplied fields for creating an Exam. UserId is intentionally
/// absent — ownership is derived server-side from the referenced Course
/// (CreateExamCommandHandler resolves and validates CourseId against
/// ICurrentUserService), never trusted from client input.
/// </summary>
public record CreateExamRequest(
    Guid CourseId,
    string Title,
    DateTime ExamDateUtc,
    string? Description);