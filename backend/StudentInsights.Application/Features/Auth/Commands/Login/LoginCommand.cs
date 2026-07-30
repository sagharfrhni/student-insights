using MediatR;
using StudentInsights.Application.Features.Auth.Common;

namespace StudentInsights.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    bool RememberMe,
    string? IpAddress) : IRequest<AuthResult>;