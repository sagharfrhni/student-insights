using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Users.DTOs;

public record UserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    UserRole Role,
    bool EmailConfirmed,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
