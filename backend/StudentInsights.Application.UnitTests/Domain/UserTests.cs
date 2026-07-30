using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.UnitTests.Domain;

public class UserTests
{
    private static User ValidUser() =>
        User.Create("Ali", "Rezaei", "ali.rezaei@example.com", "hash");

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at.example.com")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MalformedEmail_ThrowsDomainException(string email)
    {
        var act = () => User.Create("Ali", "Rezaei", email, "hash");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ValidData_NormalizesEmailToLowercase()
    {
        var user = User.Create("Ali", "Rezaei", "Ali.Rezaei@EXAMPLE.com", "hash");

        user.Email.Should().Be("ali.rezaei@example.com");
    }

    [Fact]
    public void CanLogIn_ReflectsIsActiveOnly_RegardlessOfEmailConfirmed()
    {
        // Documents the actual current behavior: EmailConfirmed is
        // deliberately NOT part of this check (see the commented-out line
        // in User.cs). A test asserting the commented-out behavior would be
        // asserting against a landmine, not the real contract.
        var user = ValidUser();

        user.EmailConfirmed.Should().BeFalse();
        user.CanLogIn().Should().BeTrue();

        user.Deactivate();
        user.CanLogIn().Should().BeFalse();
    }

    [Fact]
    public void ConfirmEmail_WrongTokenHash_ThrowsDomainException()
    {
        var user = ValidUser();
        user.SetEmailConfirmationToken("correct-hash", DateTime.UtcNow.AddHours(1));

        var act = () => user.ConfirmEmail("wrong-hash");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ConfirmEmail_CorrectToken_SetsEmailConfirmed()
    {
        var user = ValidUser();
        user.SetEmailConfirmationToken("correct-hash", DateTime.UtcNow.AddHours(1));

        user.ConfirmEmail("correct-hash");

        user.EmailConfirmed.Should().BeTrue();
        user.EmailConfirmationTokenHash.Should().BeNull();
    }

    [Fact]
    public void ConfirmEmail_AlreadyConfirmed_IsANoOp()
    {
        var user = ValidUser();
        user.SetEmailConfirmationToken("correct-hash", DateTime.UtcNow.AddHours(1));
        user.ConfirmEmail("correct-hash");

        var act = () => user.ConfirmEmail("correct-hash");

        act.Should().NotThrow();
    }

    [Fact]
    public async Task ConfirmEmail_ExpiredToken_ThrowsDomainException()
    {
        var user = ValidUser();
        user.SetEmailConfirmationToken("correct-hash", DateTime.UtcNow.AddMilliseconds(50));
        await Task.Delay(100);

        var act = () => user.ConfirmEmail("correct-hash");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ResetPassword_WrongTokenHash_ThrowsDomainException()
    {
        var user = ValidUser();
        user.SetPasswordResetToken("correct-hash", DateTime.UtcNow.AddHours(1));

        var act = () => user.ResetPassword("wrong-hash", "new-hash");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ResetPassword_CorrectToken_ChangesPasswordAndClearsToken()
    {
        var user = ValidUser();
        user.SetPasswordResetToken("correct-hash", DateTime.UtcNow.AddHours(1));

        user.ResetPassword("correct-hash", "new-hash");

        user.PasswordHash.Should().Be("new-hash");
        user.PasswordResetTokenHash.Should().BeNull();
    }

    [Fact]
    public void ChangeEmail_ToNewAddress_ClearsExistingConfirmation()
    {
        var user = ValidUser();
        user.SetEmailConfirmationToken("hash", DateTime.UtcNow.AddHours(1));
        user.ConfirmEmail("hash");

        user.ChangeEmail("new.address@example.com");

        user.Email.Should().Be("new.address@example.com");
        user.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void ChangeEmail_SameNormalizedAddress_IsANoOpAndKeepsConfirmation()
    {
        var user = ValidUser();
        user.SetEmailConfirmationToken("hash", DateTime.UtcNow.AddHours(1));
        user.ConfirmEmail("hash");

        user.ChangeEmail("ALI.REZAEI@example.com");

        user.EmailConfirmed.Should().BeTrue();
    }
}