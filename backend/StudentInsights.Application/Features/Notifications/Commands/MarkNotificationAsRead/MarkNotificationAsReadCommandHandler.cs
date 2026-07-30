using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Notifications.DTOs;
using StudentInsights.Application.Features.Notifications.Mappings;

namespace StudentInsights.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, NotificationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<NotificationDto> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        // Same 404-for-both-cases reasoning as every other handler in the
        // project (e.g. DeleteGoalCommandHandler): a missing notification
        // and one owned by someone else must be indistinguishable to the
        // caller.
        if (notification is null || notification.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Notification '{request.NotificationId}' was not found.");

        // MarkAsRead() is already idempotent (no-op if already read), so
        // no separate check is needed here — calling it twice returns
        // 200 both times.
        notification.MarkAsRead();

        await _context.SaveChangesAsync(cancellationToken);

        return notification.ToDto();
    }
}