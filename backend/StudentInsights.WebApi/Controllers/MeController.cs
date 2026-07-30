using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Features.Users.Commands.ChangePassword;
using StudentInsights.Application.Features.Users.Commands.UpdateMyProfile;
using StudentInsights.Application.Features.Users.DTOs;
using StudentInsights.Application.Features.Users.Queries.GetMyProfile;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Self-service account management for the currently authenticated user:
/// viewing and editing their own profile, and changing their password
/// while logged in. Every endpoint requires authentication and always acts
/// on the caller's own account (from ICurrentUserService) -- there is no
/// id in any route here. This controller contains no business logic — it
/// only translates HTTP requests into MediatR commands/queries and MediatR
/// results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets the profile of the currently authenticated user.</summary>
    /// <returns>The current user's profile.</returns>
    /// <response code="200">The profile was retrieved.</response>
    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var profile = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);

        return Ok(profile);
    }

    /// <summary>Updates the first name, last name, and email of the currently authenticated user.</summary>
    /// <param name="request">The user's new first name, last name, and email.</param>
    /// <returns>The updated profile.</returns>
    /// <response code="200">The profile was updated.</response>
    /// <response code="400">The request failed validation, or the email is already in use by another account.</response>
    [HttpPut]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMyProfileCommand(request.FirstName, request.LastName, request.Email);
        var profile = await _mediator.Send(command, cancellationToken);

        return Ok(profile);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user. Revokes
    /// all of the user's active refresh tokens, the same way a
    /// forgot-password reset does, so other logged-in sessions require a
    /// fresh login.
    /// </summary>
    /// <param name="request">The user's current password and desired new password.</param>
    /// <response code="204">The password was changed.</response>
    /// <response code="400">The request failed validation, or the current password is incorrect.</response>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
