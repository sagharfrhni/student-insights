using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllNotificationsAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        // Not AsNoTracking(): each entity is mutated in the loop below,
        // so change tracking is required for SaveChangesAsync to persist
        // the result. For a realistic per-user notification volume
        // (dozens, not thousands), a single SaveChangesAsync after the
        // loop is simpler and fast enough — an ExecuteUpdateAsync
        // bulk update would bypass MarkAsRead()'s ReadAtUtc stamping and
        // the audit UpdatedAtUtc interceptor, and this project has no
        // established convention for bypassing entity methods for a
        // performance win at this scale.
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == _currentUserService.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return unreadNotifications.Count;
    }
}