using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUserService.UserId)
            throw new DomainException("An administrator cannot deactivate their own account.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        user.Deactivate();

        // Mirrors ResetPasswordCommandHandler: a deactivated user's refresh
        // attempts are already blocked (RefreshTokenCommandHandler checks
        // User.CanLogIn()), but explicitly revoking here keeps the token
        // rows themselves consistent with the account's actual state,
        // instead of leaving them sitting as "active" until they expire.
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke();

        await _context.SaveChangesAsync(cancellationToken);
    }
}