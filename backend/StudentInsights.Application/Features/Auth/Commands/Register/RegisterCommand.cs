using MediatR;

namespace StudentInsights.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<RegisterResult>;

public record RegisterResult(Guid UserId, string Email);