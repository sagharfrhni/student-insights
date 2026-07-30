using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private const string GenericError = "Invalid or expired password reset request.";

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        PasswordPolicy.EnsureValid(request.NewPassword);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Same generic message whether the email isn't registered or the token
        // is wrong/expired — mirrors ForgotPasswordCommandHandler's intent of
        // never revealing whether an email is registered.
        if (user is null)
            throw new DomainException(GenericError);

        var tokenHash = SecureTokenGenerator.Hash(request.Token);
        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        try
        {
            user.ResetPassword(tokenHash, newPasswordHash);
        }
        catch (DomainException)
        {
            throw new DomainException(GenericError);
        }

        // A password reset invalidates any existing sessions.
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke();

        await _context.SaveChangesAsync(cancellationToken);
    }
}