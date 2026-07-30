using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StudentInsights.WebApi.Extensions;

/// <summary>
/// Formats a HealthReport as JSON instead of ASP.NET Core's default
/// plain-text "Healthy"/"Unhealthy" body, so GET /health responds in the
/// same application/json shape as every other endpoint in this API (see
/// ExceptionHandlingMiddleware for the equivalent contract on the error
/// path). Wired up as the ResponseWriter for the health check endpoint in
/// Program.cs.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteResponseAsync(HttpContext httpContext, HealthReport healthReport)
    {
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            status = healthReport.Status.ToString(),
            totalDurationMs = healthReport.TotalDuration.TotalMilliseconds,
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds
            })
        };

        return httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}