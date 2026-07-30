namespace StudentInsights.Application.Features.Goals.DTOs;

/// <summary>
/// User-supplied fields for updating a Goal. The goal being updated comes
/// from the route/command, not this payload -- same convention as
/// UpdateCourseRequest/UpdateLearningActivityRequest.
///
/// Deliberately excludes:
/// - Type / RelatedActivityId: immutable after creation -- Goal exposes
///   no domain method that changes either, the same way LearningActivity's
///   Type is immutable by design.
/// - CurrentValue: goes through the dedicated PATCH /goals/{id}/progress
///   endpoint (UpdateGoalProgressRequest) instead, so a full-details edit
///   can never accidentally smuggle in a progress change and vice versa
///   -- same separation used for UpdateLearningActivityStatusRequest.
/// </summary>
public record UpdateGoalRequest(decimal TargetValue, DateTime? TargetDateUtc);