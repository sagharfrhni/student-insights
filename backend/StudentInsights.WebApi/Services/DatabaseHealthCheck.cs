using Microsoft.Extensions.Diagnostics.HealthChecks;
using StudentInsights.Infrastructure.Persistence;

namespace StudentInsights.WebApi.Services;

/// <summary>
/// Reports whether the API can currently reach its SQL Server database.
/// Registered under the "database" name (see Program.cs) and surfaced at
/// GET /health so uptime monitors and, in a containerized deployment,
/// orchestrator liveness/readiness probes can distinguish "the process is
/// running" from "the process can actually serve requests" — the database
/// is the only external dependency this API has that can fail
/// independently of the process itself (Hangfire and email are best-effort
/// background/outbound concerns, not on the request path this check cares
/// about).
///
/// Depends on the concrete ApplicationDbContext, not IApplicationDbContext:
/// Database.CanConnectAsync() is an EF Core/Infrastructure concern with no
/// business meaning, so it deliberately does not go through the
/// Application-layer abstraction that every CQRS handler uses.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("The database is reachable.")
                : HealthCheckResult.Unhealthy("The database could not be reached.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("The database could not be reached.", ex);
        }
    }
}