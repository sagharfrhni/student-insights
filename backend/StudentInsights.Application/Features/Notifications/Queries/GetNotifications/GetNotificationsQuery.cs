using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Notifications.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>
/// IsRead/Type are both optional filters, same shape as GetExamsQuery's
/// CourseId/From/To — the base filter below always scopes to the
/// current user regardless of which (if any) are supplied.
/// </summary>
public record GetNotificationsQuery(
    PaginationParams Pagination,
    bool? IsRead = null,
    NotificationType? Type = null) : IRequest<PaginatedResult<NotificationDto>>;