using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Admin.Users.DTOs;

namespace StudentInsights.Application.Features.Admin.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<AdminUserListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AdminUserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();

            query = query.Where(u =>
                EF.Functions.Like(u.Email, $"%{term}%") ||
                EF.Functions.Like(u.FirstName, $"%{term}%") ||
                EF.Functions.Like(u.LastName, $"%{term}%"));
        }

        if (request.Role is not null)
            query = query.Where(u => u.Role == request.Role);

        if (request.IsActive is not null)
            query = query.Where(u => u.IsActive == request.IsActive);

        var projectedQuery = query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new AdminUserListItemDto(
                u.Id, u.FirstName, u.LastName, u.Email,
                u.Role, u.IsActive, u.EmailConfirmed, u.CreatedAtUtc));

        return await PaginatedResult<AdminUserListItemDto>.CreateAsync(
            projectedQuery,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);
    }
}