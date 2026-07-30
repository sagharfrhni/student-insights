using StudentInsights.Application.Features.Users.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Users.Mappings;

public static class UserProfileMappingExtensions
{
    public static UserProfileDto ToProfileDto(this User user)
    {
        return new UserProfileDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.Role,
            user.EmailConfirmed,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }
}
