using MediatR;
using StudentInsights.Application.Features.Admin.Users.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Users.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(Guid UserId, UserRole NewRole) : IRequest<AdminUserDetailDto>;