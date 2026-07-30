using MediatR;
using StudentInsights.Application.Features.Users.DTOs;

namespace StudentInsights.Application.Features.Users.Commands.UpdateMyProfile;

/// <summary>
/// The user being updated always comes from ICurrentUserService, not this
/// payload -- same convention as UpdateGoalCommand/UpdateCourseCommand
/// taking the target id from the route rather than the body. Password is
/// deliberately absent -- that goes through ChangePasswordCommand instead.
/// </summary>
public record UpdateMyProfileCommand(
    string FirstName,
    string LastName,
    string Email) : IRequest<UserProfileDto>;
