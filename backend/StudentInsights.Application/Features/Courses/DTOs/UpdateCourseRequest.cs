namespace StudentInsights.Application.Features.Courses.DTOs;

/// <summary>
/// User-supplied fields for updating a Course. Same shape as
/// CreateCourseRequest by design (both are the full set of editable,
/// user-owned fields on Course); the CourseId being updated comes from the
/// route/command, not from this payload.
/// </summary>
public record UpdateCourseRequest(
    string Name,
    int Credits,
    string? InstructorName);