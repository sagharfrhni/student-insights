// StudentInsights.Application/Features/Exams/DTOs/ExamDto.cs
namespace StudentInsights.Application.Features.Exams.DTOs;

/// <summary>
/// Read model for an Exam, returned from queries. CourseName is a
/// denormalized display field (from Exam.Course.Name) so API consumers
/// don't need a second round-trip just to show which course an exam
/// belongs to. Grade is projected out of the Grade value object as a
/// plain decimal?, the same flattening CourseDto.FinalGrade uses. Set
/// via the dedicated PATCH /{id}/grade endpoint (RecordExamGradeCommand),
/// same separation UpdateGoalProgressCommand uses versus UpdateGoalCommand.
/// </summary>
public record ExamDto(
    Guid Id,
    Guid CourseId,
    string CourseName,
    string Title,
    DateTime ExamDateUtc,
    string? Description,
    decimal? Grade,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);