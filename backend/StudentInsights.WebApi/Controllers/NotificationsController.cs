using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using StudentInsights.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using StudentInsights.Application.Features.Notifications.DTOs;
using StudentInsights.Application.Features.Notifications.Queries.GetNotifications;
using StudentInsights.Domain.Enums;

namespace StudentInsights.WebApi.Controllers;

/// <summary>
/// Exposes the authenticated user's generated notifications. Every
/// endpoint requires authentication; ownership of a given Notification is
/// enforced inside the corresponding Application-layer handler, not here.
/// This controller contains no business logic — it only translates HTTP
/// requests into MediatR commands/queries and MediatR results into HTTP
/// responses.
///
/// There is no POST here — Notification has no API-reachable create; the
/// only write path is NotificationGenerationJob (see BackgroundJobs),
/// which runs entirely outside the HTTP pipeline. See the module roadmap,
/// §5, for the full reasoning.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paged list of the current user's notifications, newest
    /// first, optionally filtered by read status and/or type.
    /// </summary>
    /// <param name="isRead">Optional read-status filter.</param>
    /// <param name="type">Optional notification type filter.</param>
    /// <param name="pagination">Page number and page size.</param>
    /// <returns>A page of notifications.</returns>
    /// <response code="200">The page was retrieved.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] NotificationType? type,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var notifications = await _mediator.Send(
            new GetNotificationsQuery(pagination, isRead, type), cancellationToken);

        return Ok(notifications);
    }

    /// <summary>Marks a single notification owned by the current user as read.</summary>
    /// <param name="id">The notification id.</param>
    /// <returns>The updated notification.</returns>
    /// <response code="200">The notification was marked as read (or was already read).</response>
    /// <response code="404">The notification does not exist or is not owned by the current user.</response>
    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkNotificationAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await _mediator.Send(new MarkNotificationAsReadCommand(id), cancellationToken);

        return Ok(notification);
    }

    /// <summary>Marks all of the current user's unread notifications as read.</summary>
    /// <returns>The number of notifications marked as read.</returns>
    /// <response code="200">The notifications were marked as read.</response>
    [HttpPatch("read-all")]
    public async Task<ActionResult<MarkAllNotificationsAsReadResponse>> MarkAllNotificationsAsRead(
        CancellationToken cancellationToken)
    {
        var markedCount = await _mediator.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken);

        return Ok(new MarkAllNotificationsAsReadResponse(markedCount));
    }
}