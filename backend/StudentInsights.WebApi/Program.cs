using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StudentInsights.Application.Common.Behaviors;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Auth.Commands.Register;
using StudentInsights.Infrastructure.BackgroundJobs;
using StudentInsights.Infrastructure.Email;
using StudentInsights.Infrastructure.Persistence;
using StudentInsights.Infrastructure.Security;
using StudentInsights.WebApi.Extensions;
using StudentInsights.WebApi.Middleware;
using StudentInsights.WebApi.Serialization;
using StudentInsights.WebApi.Services;
using Microsoft.OpenApi.Models;
using Hangfire.Logging;
using Microsoft.Extensions.Options;

// Bootstrap logger — active only until the host (and its configuration,
// DI container, appsettings.json "Serilog" section) finishes building.
// Console-only and deliberately minimal: its one job is to make sure a
// failure during host startup itself (bad connection string, missing
// config, DI misconfiguration) is logged somewhere before the process
// exits, rather than silently disappearing. Replaced by the fully
// configured logger the moment builder.Host.UseSerilog(...) runs below.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting StudentInsights API");

    var builder = WebApplication.CreateBuilder(args);

    // Two-stage initialization (the pattern Serilog itself recommends for
    // ASP.NET Core): reconfigures Log.Logger from the "Serilog" section of
    // appsettings.json/appsettings.{Environment}.json once configuration
    // and DI are available, and enriches every event with LogContext
    // properties — this is what makes the CorrelationId that
    // CorrelationIdMiddleware pushes per-request actually show up in the
    // sinks below. The existing structured ILogger<T> call sites
    // throughout this codebase (ExceptionHandlingMiddleware,
    // NotificationGenerationJob) don't change at all — they were already
    // Serilog-ready structured logging, just running through the default
    // console provider instead of a sink that can persist/search it.
    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // See UtcDateTimeConverter for the full rationale: normalizes every
            // DateTime crossing the API boundary (request and response) to
            // DateTimeKind.Utc, matching this project's *Utc naming convention.
            options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        });
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "StudentInsights API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // CORS — required because the React frontend (per the architecture doc,
    // §9) is a separately hosted SPA, never served from this API's origin.
    // Named policy, origins/headers/methods pulled from configuration rather
    // than hard-coded, so the allowed origin(s) can differ between
    // Development (Vite's localhost port) and Production (the deployed
    // frontend URL) without a code change.
    const string FrontendCorsPolicy = "FrontendCorsPolicy";
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(FrontendCorsPolicy, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            // No AllowCredentials(): the API is Bearer-token authenticated,
            // not cookie-authenticated, so browsers never need to send
            // credentials cross-origin for this API.
        });
    });

    // Rate limiting — ASP.NET Core 8's built-in limiter (ships in the
    // framework, no extra package). Two tiers:
    //   - "Auth" policy: a strict per-IP limit applied only to
    //     POST /api/auth/login and POST /api/auth/register (see
    //     AuthController) — the two endpoints exposed to credential-stuffing
    //     and registration-spam bots. 5 requests/minute is generous enough
    //     for a genuine user who mistypes a password a couple of times, but
    //     closes the brute-force gap.
    //   - Global limiter: a much looser per-IP baseline (100/min) for every
    //     other endpoint, as a generic abuse backstop — well above any normal
    //     dashboard/CRUD usage pattern. /health is explicitly exempted so
    //     uptime monitors and orchestrator probes are never throttled.
    // Both partition by client IP via HttpContext.GetClientIpAddress(), the
    // same X-Forwarded-For-aware helper AuthController uses for its audit
    // trail, so IP resolution stays consistent everywhere it matters.
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.GetClientIpAddress() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            // /health: uptime monitors/orchestrator probes must never be
            // throttled. /hangfire: already gated by its own Basic Auth
            // (see HangfireDashboardAuthorizationFilter); its dashboard
            // polls stats every few seconds by design, so counting it
            // against a generic abuse limit risks locking an admin out of
            // the one tool they need mid-incident, for no real security
            // benefit on top of the auth it already has.
            if (httpContext.Request.Path.StartsWithSegments("/health") ||
                httpContext.Request.Path.StartsWithSegments("/hangfire"))
            {
                return RateLimitPartition.GetNoLimiter("exempt");
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.GetClientIpAddress() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });
    });

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure()));

    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApplicationDbContext>());

    // Health checks — a lightweight liveness/readiness probe for uptime
    // monitoring and, in a containerized deployment, orchestrator health
    // probes. The database is the only external dependency on the request
    // path that can fail independently of the process itself, so that's the
    // one check registered (see DatabaseHealthCheck).
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    // Hangfire — recurring background jobs (currently just Notification
    // generation, see BackgroundJobs/NotificationGenerationJob.cs). Reuses
    // the same DefaultConnection SQL Server database as ApplicationDbContext
    // rather than a second connection string: Hangfire creates its own
    // "HangFire" schema inside the existing database, so no new database is
    // introduced for this module.
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

    builder.Services.AddHangfireServer();

    // MediatR — scans the assembly containing RegisterCommand for all handlers.
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    // FluentValidation — scans the same assembly for every IValidator<T>
    // (e.g. CreateCourseCommandValidator), picked up automatically by
    // ValidationBehavior above.
    builder.Services.AddValidatorsFromAssembly(typeof(RegisterCommand).Assembly);

    // Auth-related settings & services — validated at startup instead of failing lazily.
    builder.Services.AddOptions<JwtSettings>()
        .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
        .Validate(s => !string.IsNullOrWhiteSpace(s.Secret) && s.Secret.Length >= 32,
            "Jwt:Secret must be set and at least 32 characters (256 bits) via User Secrets or environment variables.")
        .ValidateOnStart();

    builder.Services.AddOptions<EmailSettings>()
        .Bind(builder.Configuration.GetSection(EmailSettings.SectionName))
        .Validate(s => !string.IsNullOrWhiteSpace(s.SmtpHost), "Email:SmtpHost must be configured.")
        .ValidateOnStart();

    // Unlike Jwt/Email above, InitialAdmin is optional bootstrap convenience
    // (see InitialAdminSeeder), so it is bound with no .Validate/.ValidateOnStart --
    // leaving it unconfigured is a supported state, not a startup failure.
    builder.Services.AddOptions<InitialAdminSettings>()
        .Bind(builder.Configuration.GetSection(InitialAdminSettings.SectionName));


    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Bootstrap the first Administrator account (idempotent).
    // Safe to run on every startup:
    // - Does nothing if InitialAdmin is not configured.
    // - Does nothing if an Admin already exists.
    // - Does nothing if the configured email is already registered.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        await InitialAdminSeeder.SeedAsync(
            services.GetRequiredService<IApplicationDbContext>(),
            services.GetRequiredService<IPasswordHasher>(),
            services.GetRequiredService<IOptions<InitialAdminSettings>>().Value,
            CancellationToken.None);
    }

    // Configure the HTTP request pipeline.

    // Registered first (outermost) so its headers apply to every response —
    // including error responses written further down by
    // ExceptionHandlingMiddleware — regardless of which middleware ends up
    // producing the response (see SecurityHeadersMiddleware for how).
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // Must run before ExceptionHandlingMiddleware so the correlation ID is
    // already in Serilog's LogContext by the time that middleware logs a
    // caught exception (see CorrelationIdMiddleware for the full rationale).
    //
    // Serilog's own request-logging middleware (UseSerilogRequestLogging) is
    // intentionally NOT added here: it would log a second, separate
    // "HTTP GET /api/courses responded 200" line per request alongside this
    // project's existing per-handler structured logs, duplicating
    // information rather than adding to it. CorrelationIdMiddleware gives
    // every one of those existing log lines a shared, traceable ID instead —
    // the actual gap this phase set out to close.
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Same Development-only gating already used for Swagger above: HSTS
    // sends a Strict-Transport-Security header instructing browsers to
    // only ever use HTTPS for this host, which is actively unhelpful on
    // localhost during development.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseCors(FrontendCorsPolicy);

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint — deliberately anonymous (no [Authorize], and not
    // behind app.UseAuthorization() the way controllers are): uptime monitors
    // and orchestrator probes must be able to check this without a JWT.
    // ResponseWriter formats the result as JSON instead of ASP.NET Core's
    // default plain-text body, matching the rest of this API's contract.
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
    });

    // Hangfire dashboard, gated by HangfireDashboardAuthorizationFilter (see
    // that class for the full rationale) — never left open in Production.
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthorizationFilter(app.Environment, app.Configuration) }
    });

    // Registering a recurring job is itself idempotent — re-running this on
    // every app start just re-registers the same schedule under the same
    // job id, it does not create duplicates — so no separate seed/migration
    // step is needed for it.
    RecurringJob.AddOrUpdate<NotificationGenerationJob>(
        "notification-generation",
        job => job.RunAsync(CancellationToken.None),
        Cron.Hourly);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is thrown by `dotnet ef` when it spins the host
    // up just far enough to read DI-registered services (e.g. the
    // ApplicationDbContext) for migrations tooling, then deliberately
    // aborts startup — that's expected tooling behavior, not a real
    // startup failure, so it's excluded here to avoid logging a false
    // "terminated unexpectedly" alarm every time a migration is generated.
    Log.Fatal(ex, "StudentInsights API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

// Exposes the otherwise-internal top-level-statements Program class so
// WebApplicationFactory<Program> (used by the WebApi integration test
// project to boot this API in-process) can see it. One-line,
// non-behavioral change — nothing above this point is affected.
public partial class Program
{
}