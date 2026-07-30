using MediatR;

namespace StudentInsights.Application.Features.Admin.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest;