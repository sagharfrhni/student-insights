namespace StudentInsights.WebApi.Middleware;

/// <summary>
/// Adds standard, low-risk security response headers to every response:
/// X-Content-Type-Options, X-Frame-Options, and Referrer-Policy. HSTS is
/// deliberately NOT set here — app.UseHsts() (Program.cs) already covers
/// Strict-Transport-Security and is the framework-idiomatic way to do so,
/// so duplicating it here would just be dead code with two sources of
/// truth. A Content-Security-Policy header is also deliberately omitted:
/// this is a pure JSON API with no HTML views, so a CSP has no surface to
/// protect and would be cargo-culted rather than load-bearing.
///
/// Headers are applied via Response.OnStarting rather than written
/// directly before calling _next(), so they land on every response —
/// including ones written further down the pipeline by
/// ExceptionHandlingMiddleware — regardless of where in the pipeline this
/// middleware is registered.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            return Task.CompletedTask;
        });

        return _next(context);
    }
}