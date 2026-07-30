using Serilog.Context;

namespace StudentInsights.WebApi.Middleware;

/// <summary>
/// Reads the caller-supplied X-Correlation-Id request header, or generates
/// a new one if absent, then: (1) stores it on HttpContext.Items for any
/// downstream code that wants it directly, (2) pushes it onto Serilog's
/// LogContext for the duration of the request so every structured log line
/// written anywhere during that request — including the ones
/// ExceptionHandlingMiddleware already writes for NotFoundException,
/// DbUpdateConcurrencyException, etc. — carries the same CorrelationId
/// property, and (3) echoes it back on the response header, so a client
/// (or a tester in Swagger) can hand a support/bug report a single ID that
/// ties the failed request to its exact server-side log lines.
///
/// Registered before ExceptionHandlingMiddleware in Program.cs — generating
/// the ID here, in a separate, earlier middleware, rather than inside the
/// exception middleware itself, is what makes successful (non-exception)
/// requests get one too.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) &&
                             !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}