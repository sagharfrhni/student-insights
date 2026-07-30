namespace StudentInsights.Application.Features.Notifications.DTOs;

/// <summary>
/// Response shape for PATCH /api/notifications/read-all. A named record
/// instead of returning a bare int so the count is self-describing in
/// the response body ({ "markedCount": 12 }) and produces a proper named
/// schema in Swagger, consistent with every other endpoint's documented
/// response shape.
/// </summary>
public record MarkAllNotificationsAsReadResponse(int MarkedCount);