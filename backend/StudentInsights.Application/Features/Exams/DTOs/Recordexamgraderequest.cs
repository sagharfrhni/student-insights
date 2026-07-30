// StudentInsights.Application/Features/Exams/DTOs/RecordExamGradeRequest.cs
namespace StudentInsights.Application.Features.Exams.DTOs;

/// <summary>
/// User-supplied payload for the dedicated grade-recording endpoint
/// (PATCH /{id}/grade), mirroring RecordCourseGradeRequest and the
/// narrow, single-purpose PATCH /{id}/status shape used by
/// LearningActivities. Kept separate from UpdateExamRequest so a
/// full-details edit can never accidentally smuggle in a grade change,
/// and vice versa.
/// </summary>
public record RecordExamGradeRequest(decimal Grade);