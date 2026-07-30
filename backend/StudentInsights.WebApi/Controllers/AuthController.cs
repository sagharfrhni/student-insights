using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudentInsights.Application.Features.Auth.Commands.ConfirmEmail;
using StudentInsights.Application.Features.Auth.Commands.ForgotPassword;
using StudentInsights.Application.Features.Auth.Commands.Login;
using StudentInsights.Application.Features.Auth.Commands.Logout;
using StudentInsights.Application.Features.Auth.Commands.Register;
using StudentInsights.Application.Features.Auth.Commands.ResetPassword;
using StudentInsights.WebApi.Extensions;
using RefreshTokenCommand = StudentInsights.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand;

namespace StudentInsights.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ActionResult<RegisterResult>> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.RememberMe, HttpContext.GetClientIpAddress());
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, HttpContext.GetClientIpAddress());
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken, HttpContext.GetClientIpAddress());
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return Ok("Email confirmed successfully.");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}

public record LoginRequest(string Email, string Password, bool RememberMe);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);