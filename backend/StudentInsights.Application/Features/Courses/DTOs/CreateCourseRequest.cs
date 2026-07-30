namespace StudentInsights.Application.Features.Courses.DTOs;

/// <summary>
/// User-supplied fields for creating a Course. UserId is intentionally
/// absent — it must never be trusted from client input and is always
/// resolved server-side from ICurrentUserService by the command handler.
/// </summary>
public record CreateCourseRequest(
    string Name,
    int Credits,
    string? InstructorName);