using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Users.DTOs;
using StudentInsights.Application.Features.Users.Mappings;

namespace StudentInsights.Application.Features.Users.Queries.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: this is a pure read, the entity is never mutated
        // or saved, so there's no reason to pay for EF's change tracking.
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        // Every consumer runs behind [Authorize], so this only happens if the
        // authenticated user was deleted after the access token was issued.
        if (user is null)
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");

        return user.ToProfileDto();
    }
}
