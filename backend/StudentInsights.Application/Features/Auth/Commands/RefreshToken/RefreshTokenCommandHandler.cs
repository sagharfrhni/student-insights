using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Application.Features.Auth.Common;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = SecureTokenGenerator.Hash(request.RefreshToken);

        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
            throw new DomainException("Invalid refresh token.");

        if (existingToken.IsRevoked)
        {
            // A revoked (already-rotated) token was presented again --- treat
            // this as a possible compromise and kill every active session
            // for this user.
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == existingToken.UserId && rt.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
                token.Revoke(request.IpAddress);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another request already revoked these rows first --- the end
                // state we wanted is already achieved, nothing further to do.
            }

            throw new DomainException("Invalid refresh token.");
        }

        if (existingToken.IsExpired)
            throw new DomainException("Refresh token has expired.");

        // The global soft-delete query filter applies to Include()'d navigations
        // too, so a token belonging to a soft-deleted user comes back with
        // User == null rather than being excluded outright. Treat that the
        // same as "invalid token" instead of letting it NullReferenceException.
        var user = existingToken.User;
        if (user is null || !user.CanLogIn())
            throw new DomainException("Invalid refresh token.");

        var (accessToken, accessTokenExpiresAtUtc) = _jwtTokenGenerator.GenerateAccessToken(user);

        var rawNewRefreshToken = SecureTokenGenerator.GenerateToken();
        var newTokenHash = SecureTokenGenerator.Hash(rawNewRefreshToken);

        // Preserve the original session length rather than resetting it ---
        // rotation keeps the token fresh without silently extending "remember me".
        var newRefreshToken = StudentInsights.Domain.Entities.RefreshToken.Create(
            user, newTokenHash, existingToken.ExpiresAtUtc, request.IpAddress);

        existingToken.Revoke(request.IpAddress, newTokenHash);
        _context.RefreshTokens.Add(newRefreshToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request rotated this exact token first (e.g. a duplicate
            // or racing refresh call). Treat this attempt as if the token had
            // already been used --- the client should retry with whichever
            // response won the race.
            throw new DomainException("Invalid refresh token.");
        }

        return new AuthResult(accessToken, accessTokenExpiresAtUtc, rawNewRefreshToken, existingToken.ExpiresAtUtc);
    }
}