using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Application.Features.Auth.Common;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using RefreshTokenEntity = StudentInsights.Domain.Entities.RefreshToken;

namespace StudentInsights.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private const int DefaultRefreshTokenDays = 7;
    private const int RememberMeRefreshTokenDays = 30;

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    // Lazily computed once and cached: a valid hash of a value nobody will ever
    // type. Verifying against it when no matching user is found keeps the
    // "unknown email" response taking roughly as long as "wrong password" —
    // otherwise the two are distinguishable by timing, which lets an attacker
    // enumerate registered emails without ever seeing a different message.
    private static string? _dummyPasswordHash;
    private static readonly object DummyHashLock = new();

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            _passwordHasher.Verify(request.Password, GetDummyPasswordHash());
            throw new DomainException("Invalid email or password.");
        }

        // Same message for "wrong password" and "not active/confirmed" —
        // never reveal which one it was.
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash) || !user.CanLogIn())
            throw new DomainException("Invalid email or password.");

        var (accessToken, accessTokenExpiresAtUtc) = _jwtTokenGenerator.GenerateAccessToken(user);

        var refreshDays = request.RememberMe ? RememberMeRefreshTokenDays : DefaultRefreshTokenDays;
        var rawRefreshToken = SecureTokenGenerator.GenerateToken();
        var refreshTokenHash = SecureTokenGenerator.Hash(rawRefreshToken);
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(refreshDays);

        var refreshToken = RefreshTokenEntity.Create(user, refreshTokenHash, refreshTokenExpiresAtUtc, request.IpAddress);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResult(accessToken, accessTokenExpiresAtUtc, rawRefreshToken, refreshTokenExpiresAtUtc);
    }

    private string GetDummyPasswordHash()
    {
        if (_dummyPasswordHash is null)
        {
            lock (DummyHashLock)
            {
                _dummyPasswordHash ??= _passwordHasher.Hash(Guid.NewGuid().ToString());
            }
        }

        return _dummyPasswordHash;
    }
}