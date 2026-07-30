namespace StudentInsights.Application.Features.Courses.DTOs;

/// <summary>
/// User-supplied payload for the dedicated grade-recording endpoint
/// (PATCH /{id}/grade), mirroring the narrow, single-purpose
/// PATCH /{id}/progress shape used by Goals. Kept separate from
/// UpdateCourseRequest so a full-details edit can never accidentally
/// smuggle in a grade change, and vice versa.
/// </summary>
public record RecordCourseGradeRequest(decimal Grade);