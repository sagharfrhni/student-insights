using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Admin.Users.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Users.Queries.GetUsers;

public record GetUsersQuery(
    string? Search,
    UserRole? Role,
    bool? IsActive,
    PaginationParams Pagination) : IRequest<PaginatedResult<AdminUserListItemDto>>;