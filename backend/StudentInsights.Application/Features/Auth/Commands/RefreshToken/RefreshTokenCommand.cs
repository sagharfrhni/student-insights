using MediatR;
using StudentInsights.Application.Features.Auth.Common;

namespace StudentInsights.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress) : IRequest<AuthResult>;