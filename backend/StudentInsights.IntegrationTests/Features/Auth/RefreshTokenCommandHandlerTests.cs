using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentInsights.Application.Common.Security;
using StudentInsights.Application.Features.Auth.Commands.RefreshToken;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Infrastructure.Security;
using StudentInsights.IntegrationTests.Common;

namespace StudentInsights.IntegrationTests.Features.Auth;

public class RefreshTokenCommandHandlerTests : IDisposable
{
    private readonly SqliteContextFactory _factory = new();

    private RefreshTokenCommandHandler CreateHandler() =>
        new(_factory.Context, new JwtTokenGenerator(Options.Create(new JwtSettings
        {
            Issuer = "StudentInsights.Tests",
            Audience = "StudentInsights.Tests",
            Secret = "test-secret-key-at-least-32-characters-long",
            AccessTokenMinutes = 60
        })));

    private async Task<User> SeedActiveUserAsync(string email = "ali.rezaei@example.com")
    {
        var user = User.Create("Ali", "Rezaei", email, "hash");
        _factory.Context.Users.Add(user);
        await _factory.Context.SaveChangesAsync(CancellationToken.None);
        return user;
    }

    private async Task<string> SeedActiveRefreshTokenAsync(User user, DateTime? expiresAtUtc = null)
    {
        var rawToken = SecureTokenGenerator.GenerateToken();
        var tokenHash = SecureTokenGenerator.Hash(rawToken);
        var token = RefreshToken.Create(user, tokenHash, expiresAtUtc ?? DateTime.UtcNow.AddDays(7));
        _factory.Context.RefreshTokens.Add(token);
        await _factory.Context.SaveChangesAsync(CancellationToken.None);
        return rawToken;
    }

    [Fact]
    public async Task Handle_ValidToken_RotatesIntoNewTokenAndRevokesTheOld()
    {
        var user = await SeedActiveUserAsync();
        var rawToken = await SeedActiveRefreshTokenAsync(user);
        var oldTokenHash = SecureTokenGenerator.Hash(rawToken);
        var handler = CreateHandler();

        var result = await handler.Handle(new RefreshTokenCommand(rawToken, null), CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(rawToken);

        var oldToken = await _factory.Context.RefreshTokens.FirstAsync(t => t.TokenHash == oldTokenHash);
        oldToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReplayingARevokedToken_ThrowsAndRevokesEveryActiveSessionForThatUser()
    {
        var user = await SeedActiveUserAsync();

        // Token A: already revoked (simulating "was already rotated once").
        var rawTokenA = await SeedActiveRefreshTokenAsync(user);
        var hashA = SecureTokenGenerator.Hash(rawTokenA);
        var tokenA = await _factory.Context.RefreshTokens.FirstAsync(t => t.TokenHash == hashA);
        tokenA.Revoke();
        await _factory.Context.SaveChangesAsync(CancellationToken.None);

        // Token B: a second, still-active session for the same user.
        await SeedActiveRefreshTokenAsync(user);

        var handler = CreateHandler();

        var act = () => handler.Handle(new RefreshTokenCommand(rawTokenA, null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        var stillActiveCount = await _factory.Context.RefreshTokens
            .CountAsync(t => t.UserId == user.Id && t.RevokedAtUtc == null);
        stillActiveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsWithExpiredMessage()
    {
        var user = await SeedActiveUserAsync();
        var rawToken = await SeedActiveRefreshTokenAsync(user, DateTime.UtcNow.AddMilliseconds(100));
        await Task.Delay(150);

        var handler = CreateHandler();

        var act = () => handler.Handle(new RefreshTokenCommand(rawToken, null), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.Message.Should().Be("Refresh token has expired.");
    }

    [Fact]
    public async Task Handle_TokenBelongingToDeactivatedUser_Throws()
    {
        var user = await SeedActiveUserAsync();
        var rawToken = await SeedActiveRefreshTokenAsync(user);
        user.Deactivate();
        await _factory.Context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler();

        var act = () => handler.Handle(new RefreshTokenCommand(rawToken, null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_UnknownToken_Throws()
    {
        await SeedActiveUserAsync();
        var handler = CreateHandler();

        var act = () => handler.Handle(new RefreshTokenCommand("never-issued-token", null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    public void Dispose() => _factory.Dispose();
}