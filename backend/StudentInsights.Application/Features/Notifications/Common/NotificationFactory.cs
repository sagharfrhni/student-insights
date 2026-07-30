using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Notifications.Common;

/// <summary>
/// Single choke point every generated notification passes through.
/// Introduced because NotificationGenerationJob needs something none of
/// the existing MediatR commands provide: creation that isn't triggered
/// by an HTTP request and isn't scoped to a single ICurrentUserService
/// user (see the module roadmap, §10). Static and takes
/// IApplicationDbContext + an explicit userId as parameters, no DI, no
/// HttpContext assumption — same convention as GoalProgressInputsProvider.
///
/// Never calls SaveChangesAsync — the caller is responsible for that, so
/// a batch of many notifications across many users/checks can be
/// committed in controlled chunks instead of one round trip each.
/// </summary>
public static class NotificationFactory
{
    /// <summary>
    /// Creates and stages a Notification for the given user, unless one
    /// already exists for the same (UserId, Type, SourceId) triple, in
    /// which case this is a no-op and null is returned. Because
    /// Notification inherits BaseEntity, the automatic soft-delete query
    /// filter means a soft-deleted notification does not count as
    /// "already exists" — deliberate, not accidental (see
    /// NotificationConfiguration's index comment).
    /// </summary>
    /// <param name="ignoreExistingBeforeUtc">
    /// When supplied, an existing notification only counts as a duplicate
    /// if its CreatedAtUtc is on or after this timestamp; anything older
    /// is ignored. Used by the Overdue Activity check to let a reopened,
    /// since-cleared activity that becomes overdue again receive a fresh
    /// notification: it passes the activity's LastCompletedAtUtc here, so
    /// only notifications created since that completion block a new one
    /// (see the module roadmap, §19, and NotificationGenerationJob's
    /// GenerateOverdueActivityAsync). Null (the default) preserves the
    /// base "one notification ever" rule used by every other check.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="userId"/> does not resolve to a User.
    /// Every caller is expected to have already filtered to active user
    /// ids before invoking this method, so this indicates a genuine data-
    /// integrity problem, not an ordinary "not found" — surfaced as a
    /// fail-fast exception rather than silently skipping the notification.
    /// </exception>
    /// <exception cref="StudentInsights.Domain.Common.DomainException">
    /// Propagated, unhandled, from Notification.Create — e.g. if
    /// <paramref name="message"/> is blank. Left to bubble up so the
    /// calling check-method's own try/catch (see NotificationGenerationJob)
    /// can log it with context about which check produced it.
    /// </exception>
    public static async Task<Notification?> TryCreateAsync(
        IApplicationDbContext context,
        Guid userId,
        NotificationType type,
        string message,
        Guid sourceId,
        CancellationToken cancellationToken,
        DateTime? ignoreExistingBeforeUtc = null)
    {
        var existingQuery = context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Type == type && n.SourceId == sourceId);

        if (ignoreExistingBeforeUtc is not null)
            existingQuery = existingQuery.Where(n => n.CreatedAtUtc >= ignoreExistingBeforeUtc.Value);

        var alreadyExists = await existingQuery.AnyAsync(cancellationToken);

        if (alreadyExists)
            return null;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"User '{userId}' was not found while generating a '{type}' notification. " +
                "Callers must only pass ids resolved from an active-user query.");
        }

        var notification = Notification.Create(user, type, message, sourceId);

        context.Notifications.Add(notification);

        return notification;
    }
}