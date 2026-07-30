namespace StudentInsights.Application.Features.Courses.DTOs;

/// <summary>
/// User-supplied fields for updating a Course. The CourseId being updated
/// comes from the route/command, not from this payload.
///
/// Deliberately excludes Semester: immutable after creation -- Course
/// exposes no domain method that changes it, the same way Goal's Type is
/// immutable by design (see UpdateGoalRequest).
/// </summary>
public record UpdateCourseRequest(
    string Name,
    int Credits,
    string? InstructorName);