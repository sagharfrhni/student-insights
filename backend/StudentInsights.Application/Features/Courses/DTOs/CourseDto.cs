namespace StudentInsights.Application.Features.Courses.DTOs;

/// <summary>
/// Read model for a Course, returned from queries. Deliberately flat —
/// FinalGrade is projected out of the Grade value object as a plain
/// decimal? so API consumers don't need to know about the domain's
/// value-object wrapping.
/// </summary>
public record CourseDto(
    Guid Id,
    string Name,
    int Credits,
    string? InstructorName,
    decimal? FinalGrade,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);