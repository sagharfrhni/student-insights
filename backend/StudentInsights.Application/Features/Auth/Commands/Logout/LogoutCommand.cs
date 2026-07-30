using MediatR;

namespace StudentInsights.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken, string? IpAddress) : IRequest;