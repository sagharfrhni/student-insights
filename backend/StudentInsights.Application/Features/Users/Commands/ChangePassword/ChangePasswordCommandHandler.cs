using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// No FluentValidation validator, by design -- same convention as
/// ResetPasswordCommand/RegisterCommand: password strength is enforced
/// through PasswordPolicy.EnsureValid, called manually below, rather than a
/// separate validator class.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("Current password is incorrect.");

        PasswordPolicy.EnsureValid(request.NewPassword);

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePasswordHash(newPasswordHash);

        // A password change invalidates any existing sessions -- exact same
        // reasoning and pattern as ResetPasswordCommandHandler.
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
