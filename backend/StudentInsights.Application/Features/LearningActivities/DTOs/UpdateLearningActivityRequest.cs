using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.DTOs;

/// <summary>
/// User-supplied fields for updating a LearningActivity's descriptive
/// details. The activity being updated comes from the route/command, not
/// this payload — same convention as UpdateCourseRequest.
///
/// Deliberately excludes:
/// - CourseId: an activity is not reassignable to a different course.
/// - Type: LearningActivity.UpdateDetails does not touch Type, and no
///   domain method changes it after creation — it is immutable by design,
///   same as Course.Credits/Name are only mutable through their own named
///   methods rather than a generic setter.
/// - Status / CompletedAtUtc: status changes go through the dedicated
///   PATCH /{id}/status endpoint (UpdateLearningActivityStatusRequest),
///   which is the only path allowed to move Status and, as a consequence,
///   CompletedAtUtc.
///
/// Unlike CreateLearningActivityRequest, DueDateUtc here is not validated
/// against "not in the past" — the product spec explicitly lifts that
/// restriction on edit (a user may legitimately be recording/adjusting an
/// already-overdue item).
/// </summary>
public record UpdateLearningActivityRequest(
    string Title,
    DateTime DueDateUtc,
    ActivityPriority Priority,
    string? Description,
    string? ResourceLink);