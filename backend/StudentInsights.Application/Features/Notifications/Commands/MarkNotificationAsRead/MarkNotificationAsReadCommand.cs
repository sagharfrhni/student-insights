using MediatR;
using StudentInsights.Application.Features.Notifications.DTOs;

namespace StudentInsights.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<NotificationDto>;