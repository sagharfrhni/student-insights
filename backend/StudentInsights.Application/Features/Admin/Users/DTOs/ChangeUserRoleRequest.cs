using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Users.DTOs;

public record ChangeUserRoleRequest(UserRole NewRole);