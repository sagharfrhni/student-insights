using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Users.DTOs;

public record AdminUserDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTime? EmailConfirmedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    int CourseCount,
    int LearningActivityCount,
    int ExamCount,
    int GoalCount);