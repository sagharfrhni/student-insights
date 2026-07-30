namespace StudentInsights.WebApi.Extensions;

/// <summary>
/// Named policy identifiers for ASP.NET Core rate limiting (registered via
/// AddRateLimiter in Program.cs). Centralized as constants, rather than
/// string literals duplicated between Program.cs and the controller
/// actions that apply them via [EnableRateLimiting], so the two can't
/// silently drift out of sync.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Strict per-IP limit for the credential-stuffing / registration-spam
    /// surface: POST /api/auth/login and POST /api/auth/register.
    /// </summary>
    public const string Auth = "Auth";
}