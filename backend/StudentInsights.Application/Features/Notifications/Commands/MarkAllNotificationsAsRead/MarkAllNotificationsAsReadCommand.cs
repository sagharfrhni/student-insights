using MediatR;

namespace StudentInsights.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

/// <summary>
/// Returns the count marked, so the frontend can show e.g. "12
/// notifications marked as read" without a follow-up count query.
/// </summary>
public record MarkAllNotificationsAsReadCommand : IRequest<int>;