using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Users.DTOs;

public record AdminUserListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAtUtc);