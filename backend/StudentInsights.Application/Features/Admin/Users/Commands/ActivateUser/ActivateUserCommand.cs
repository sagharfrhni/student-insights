using MediatR;

namespace StudentInsights.Application.Features.Admin.Users.Commands.ActivateUser;

public record ActivateUserCommand(Guid UserId) : IRequest;