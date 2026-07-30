namespace StudentInsights.Infrastructure.Security;

/// <summary>
/// Configuration for the one-time bootstrap Administrator account created
/// by InitialAdminSeeder. Left unset (Email/Password blank) by default --
/// like JwtSettings.Secret and EmailSettings.SmtpPassword, the real
/// values are expected to come from User Secrets or environment
/// variables, not source control. Unlike those two, an unset
/// InitialAdmin is a valid, supported state: seeding is optional
/// bootstrap convenience, not something the app depends on to run, so it
/// is simply skipped rather than failing startup.
/// </summary>
public class InitialAdminSettings
{
    public const string SectionName = "InitialAdmin";

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}