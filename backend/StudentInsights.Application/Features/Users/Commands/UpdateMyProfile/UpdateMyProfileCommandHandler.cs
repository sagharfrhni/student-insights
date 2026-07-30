using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Users.DTOs;
using StudentInsights.Application.Features.Users.Mappings;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Users.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMyProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Only check uniqueness (and only touch the domain method) when the
        // email is actually changing, so re-submitting the same email never
        // trips the "already in use" check against the user's own row, and
        // ChangeEmail's confirmation-reset side effect never fires for a
        // no-op update.
        if (normalizedEmail != user.Email)
        {
            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.Id != user.Id, cancellationToken);

            if (emailInUse)
                throw new DomainException("An account with this email already exists.");

            user.ChangeEmail(request.Email);
        }

        user.Rename(request.FirstName, request.LastName);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Same reasoning as RegisterCommandHandler: the AnyAsync check
            // above covers the common case; this catches the narrow race
            // where two updates for the same email land at nearly the same
            // instant. Re-check rather than assuming the failure was the
            // email-uniqueness violation, so an unrelated DbUpdateException
            // isn't misreported as "email already exists".
            var stillInUse = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.Id != user.Id, cancellationToken);

            if (stillInUse)
                throw new DomainException("An account with this email already exists.");

            throw;
        }

        return user.ToProfileDto();
    }
}
