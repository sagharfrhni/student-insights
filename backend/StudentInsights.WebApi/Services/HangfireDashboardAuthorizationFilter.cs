using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace StudentInsights.WebApi.Services;

/// <summary>
/// Gates the Hangfire dashboard ("/hangfire") behind HTTP Basic
/// Authentication using credentials from configuration
/// (Hangfire:DashboardUsername / Hangfire:DashboardPassword), rather
/// than the project's JWT/[Authorize] scheme. The Admin module (see
/// AdminUsersController etc.) does now have role-based
/// [Authorize(Roles = "Admin")] policies, but Basic Auth remains the
/// right call for this specific surface: it's an operational page, not
/// a user-facing one, and gating it behind a browser-friendly
/// credential prompt avoids requiring dashboard visitors to obtain and
/// attach a JWT just to check on background jobs. A lightweight,
/// self-contained Basic Auth check needs no new package
/// (IDashboardAuthorizationFilter already ships with Hangfire.AspNetCore)
/// and no new infrastructure.
///
/// In Development, every request is allowed through so the dashboard
/// stays convenient to use locally — the same reasoning already applied
/// to Swagger being Development-only in Program.cs. In every other
/// environment, a valid Basic Authorization header is required; if no
/// credentials are configured, every request is rejected outright rather
/// than allowed through — a missing configuration value must fail
/// closed, never open.
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_environment.IsDevelopment())
            return true;

        var httpContext = context.GetHttpContext();

        var configuredUsername = _configuration["Hangfire:DashboardUsername"];
        var configuredPassword = _configuration["Hangfire:DashboardPassword"];

        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
            return false;

        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        try
        {
            var encodedCredentials = authorizationHeader["Basic ".Length..].Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));

            var separatorIndex = decodedCredentials.IndexOf(':');
            if (separatorIndex < 0)
            {
                Challenge(httpContext);
                return false;
            }

            var username = decodedCredentials[..separatorIndex];
            var password = decodedCredentials[(separatorIndex + 1)..];

            var isAuthorized =
                IsEqual(username, configuredUsername) &&
                IsEqual(password, configuredPassword);

            if (!isAuthorized)
                Challenge(httpContext);

            return isAuthorized;
        }
        catch (FormatException)
        {
            // Malformed Base64 in the Authorization header — treat the
            // same as "no credentials supplied", not as a server error.
            Challenge(httpContext);
            return false;
        }
    }

    /// <summary>
    /// Constant-time comparison so a failed credential check can't leak
    /// timing information — cheap to do correctly for anything gating an
    /// operational surface like this one.
    /// </summary>
    private static bool IsEqual(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private static void Challenge(HttpContext httpContext) =>
        httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
}