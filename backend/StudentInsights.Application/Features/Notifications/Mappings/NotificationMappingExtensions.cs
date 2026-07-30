using StudentInsights.Application.Features.Notifications.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Notifications.Mappings;

/// <summary>
/// Manual Notification -&gt; NotificationDto mapping (no AutoMapper/
/// Mapster), mirroring CourseMappingExtensions/ExamMappingExtensions:
/// a single explicit method is simpler than configuring and maintaining
/// a mapping profile for a shape this small.
///
/// Unlike LearningActivityMappingExtensions.ToDto, this takes no extra
/// parameters — Notification has no cross-aggregate display field to
/// denormalize (SourceId is a plain, already-present value on the
/// entity itself), so there is nothing a caller needs to supply.
///
/// No request DTOs are mapped through here: Notification has no
/// API-reachable create/update (see the module roadmap, §5) — the only
/// write path is NotificationFactory, which calls Notification.Create(...)
/// directly, keeping the entity's own invariants the single source of
/// truth.
/// </summary>
public static class NotificationMappingExtensions
{
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Message,
            notification.IsRead,
            notification.ReadAtUtc,
            notification.SourceId,
            notification.CreatedAtUtc);
    }
}