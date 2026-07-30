using StudentInsights.Application.Features.Admin.Users.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Users.Mappings;

public static class AdminUserMappingExtensions
{
    public static AdminUserDetailDto ToDetailDto(
        this User user,
        int courseCount,
        int learningActivityCount,
        int examCount,
        int goalCount)
    {
        return new AdminUserDetailDto(
            user.Id, user.FirstName, user.LastName, user.Email,
            user.Role, user.IsActive, user.EmailConfirmed, user.EmailConfirmedAtUtc,
            user.CreatedAtUtc, user.UpdatedAtUtc,
            courseCount, learningActivityCount, examCount, goalCount);
    }
}