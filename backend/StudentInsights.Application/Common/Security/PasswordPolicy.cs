using System.Text.RegularExpressions;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Common.Security;

/// <summary>
/// Single source of truth for the password strength rule enforced at
/// registration and password reset. This is an application-level policy
/// (it can change independently of the User entity's own invariants), so it
/// deliberately lives here rather than on the domain entity --- same reasoning
/// as SecureTokenGenerator: no state, no config, no DI, just a static helper.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    private static readonly Regex LetterPattern = new("[a-zA-Z]", RegexOptions.Compiled);
    private static readonly Regex DigitPattern = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex SpecialCharPattern = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);

    public static void EnsureValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password is required.");

        if (password.Length < MinLength)
            throw new DomainException($"Password must be at least {MinLength} characters long.");

        if (!LetterPattern.IsMatch(password))
            throw new DomainException("Password must contain at least one English letter.");

        if (!DigitPattern.IsMatch(password))
            throw new DomainException("Password must contain at least one number.");

        if (!SpecialCharPattern.IsMatch(password))
            throw new DomainException("Password must contain at least one special character.");
    }
}