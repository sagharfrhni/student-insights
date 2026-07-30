using MediatR;
using StudentInsights.Application.Features.Admin.Users.DTOs;

namespace StudentInsights.Application.Features.Admin.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<AdminUserDetailDto>;