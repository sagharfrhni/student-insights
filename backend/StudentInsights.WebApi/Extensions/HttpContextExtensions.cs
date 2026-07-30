namespace StudentInsights.WebApi.Extensions;

/// <summary>
/// Resolves the caller's IP address consistently everywhere it's needed
/// (AuthController's audit trail for login/refresh/logout, and the "Auth"
/// rate limiter policy in Program.cs). Centralized here instead of
/// duplicated in both places, since both call sites need the exact same
/// reverse-proxy-aware logic to stay correct together.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Behind a reverse proxy / load balancer (the normal production
    /// topology), Connection.RemoteIpAddress is the proxy's address, not
    /// the client's. Prefer X-Forwarded-For when present.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) &&
            !string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.ToString().Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}