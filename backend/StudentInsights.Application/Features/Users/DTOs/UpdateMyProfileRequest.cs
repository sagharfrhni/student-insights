namespace StudentInsights.Application.Features.Users.DTOs;

/// <summary>
/// User-supplied fields for updating the current user's profile. The user
/// being updated always comes from ICurrentUserService, never this payload
/// -- same convention as UpdateCourseRequest/UpdateGoalRequest not accepting
/// an id.
///
/// Deliberately excludes password -- that goes through the dedicated
/// POST /me/change-password endpoint (ChangePasswordRequest) instead, so a
/// profile edit can never accidentally smuggle in a password change and
/// vice versa -- same separation used for UpdateGoalRequest/UpdateGoalProgressRequest.
/// </summary>
public record UpdateMyProfileRequest(string FirstName, string LastName, string Email);
