using MediatR;

namespace StudentInsights.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;
