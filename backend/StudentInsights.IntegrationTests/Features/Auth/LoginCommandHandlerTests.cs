using Microsoft.Extensions.Options;
using StudentInsights.Application.Features.Auth.Commands.Login;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Infrastructure.Security;
using StudentInsights.IntegrationTests.Common;

namespace StudentInsights.IntegrationTests.Features.Auth;

public class LoginCommandHandlerTests : IDisposable
{
    private const string ValidPassword = "Str0ng!Pass";

    private readonly SqliteContextFactory _factory = new();
    private readonly PasswordHasher _passwordHasher = new();

    private LoginCommandHandler CreateHandler() =>
        new(_factory.Context, _passwordHasher, new JwtTokenGenerator(Options.Create(TestJwtSettings())));

    private static JwtSettings TestJwtSettings() => new()
    {
        Issuer = "StudentInsights.Tests",
        Audience = "StudentInsights.Tests",
        Secret = "test-secret-key-at-least-32-characters-long",
        AccessTokenMinutes = 60
    };

    private async Task<User> SeedActiveUserAsync(string email = "ali.rezaei@example.com")
    {
        var user = User.Create("Ali", "Rezaei", email, _passwordHasher.Hash(ValidPassword));
        _factory.Context.Users.Add(user);
        await _factory.Context.SaveChangesAsync(CancellationToken.None);
        return user;
    }

    [Fact]
    public async Task Handle_CorrectCredentials_IssuesAccessAndRefreshTokens()
    {
        var user = await SeedActiveUserAsync();
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand(user.Email, ValidPassword, false, null), CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.AccessTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
        result.RefreshTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_RememberMe_IssuesLongerLivedRefreshToken()
    {
        var user = await SeedActiveUserAsync("remember.me@example.com");
        var handler = CreateHandler();

        var withoutRememberMe = await handler.Handle(new LoginCommand(user.Email, ValidPassword, false, null), CancellationToken.None);
        var withRememberMe = await handler.Handle(new LoginCommand(user.Email, ValidPassword, true, null), CancellationToken.None);

        withRememberMe.RefreshTokenExpiresAtUtc.Should().BeAfter(withoutRememberMe.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public async Task Handle_WrongPassword_UnknownEmail_AndInactiveUser_AllThrowTheIdenticalMessage()
    {
        var activeUser = await SeedActiveUserAsync("active@example.com");
        var inactiveUser = await SeedActiveUserAsync("inactive@example.com");
        inactiveUser.Deactivate();
        await _factory.Context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler();

        var wrongPasswordEx = await Record.ExceptionAsync(() =>
            handler.Handle(new LoginCommand(activeUser.Email, "WrongPassword1!", false, null), CancellationToken.None));

        var unknownEmailEx = await Record.ExceptionAsync(() =>
            handler.Handle(new LoginCommand("nobody@example.com", ValidPassword, false, null), CancellationToken.None));

        var inactiveUserEx = await Record.ExceptionAsync(() =>
            handler.Handle(new LoginCommand(inactiveUser.Email, ValidPassword, false, null), CancellationToken.None));

        wrongPasswordEx.Should().BeOfType<DomainException>();
        unknownEmailEx.Should().BeOfType<DomainException>();
        inactiveUserEx.Should().BeOfType<DomainException>();

        // The security property under test: none of these three cases is
        // distinguishable from another by message.
        wrongPasswordEx!.Message.Should().Be(unknownEmailEx!.Message);
        wrongPasswordEx.Message.Should().Be(inactiveUserEx!.Message);
    }

    public void Dispose() => _factory.Dispose();
}