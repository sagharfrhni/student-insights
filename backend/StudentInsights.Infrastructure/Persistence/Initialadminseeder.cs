using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;
using StudentInsights.Infrastructure.Security;

namespace StudentInsights.Infrastructure.Persistence;

/// <summary>
/// One-time startup bootstrap for the very first Administrator account,
/// called from Program.cs directly -- the same way
/// NotificationGenerationJob's recurring-job registration is -- since
/// there is no authenticated Admin caller yet to drive this through the
/// normal ChangeUserRoleCommand path.
///
/// Idempotent by design, so it is safe to run on every app start:
/// - No-ops if InitialAdminSettings is left unconfigured (Email/Password
///   blank) -- the same "optional, silently skipped" convention as any
///   other unconfigured feature.
/// - No-ops if an Administrator already exists anywhere in the system --
///   this seeder only ever creates the *first* one; every subsequent
///   promotion goes through ChangeUserRoleCommand, same as the rest of
///   the Admin module.
/// - No-ops if the configured email is already registered, so it never
///   overwrites or duplicates an existing account.
/// Reuses PasswordPolicy/IPasswordHasher/User.Create exactly as
/// RegisterCommandHandler does, so the bootstrap admin is held to the
/// same password and profile rules as every other account.
/// </summary>
public static class InitialAdminSeeder
{
    public static async Task SeedAsync(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        InitialAdminSettings settings,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
            return;

        var adminExists = await context.Users
            .AnyAsync(u => u.Role == UserRole.Admin, cancellationToken);

        if (adminExists)
            return;

        var normalizedEmail = settings.Email.Trim().ToLowerInvariant();

        var emailInUse = await context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailInUse)
            return;

        PasswordPolicy.EnsureValid(settings.Password);

        var passwordHash = passwordHasher.Hash(settings.Password);
        var admin = User.Create(settings.FirstName, settings.LastName, settings.Email, passwordHash, UserRole.Admin);

        context.Users.Add(admin);

        await context.SaveChangesAsync(cancellationToken);
    }
}