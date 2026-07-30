using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Notifications.DTOs;

/// <summary>
/// Read model for a Notification, returned from queries. No UpdatedAtUtc
/// field, unlike CourseDto/ExamDto/LearningActivityDto/GoalDto: those
/// entities are genuinely editable after creation, but Notification has
/// no Update() method by design — if the underlying condition changes
/// (e.g. a deadline moves further out), the correct outcome is a new
/// notification on the next generation run, not an edited old one (see
/// the Notification entity's own doc comments). Exposing UpdatedAtUtc
/// here would therefore only ever equal ReadAtUtc or be null, adding no
/// information ReadAtUtc doesn't already carry.
///
/// SourceId is passed through as-is (untyped, per Notification's own doc
/// comment) — interpreting it into a concrete deep link (e.g.
/// ExamTomorrow -&gt; /exams/{sourceId}) is left to the frontend, which
/// already has Type to key that decision on.
/// </summary>
public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Message,
    bool IsRead,
    DateTime? ReadAtUtc,
    Guid? SourceId,
    DateTime CreatedAtUtc);