using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.DTOs;

/// <summary>
/// User-supplied fields for creating a Goal. UserId is deliberately
/// absent -- same convention as CreateCourseRequest/CreatePersonalEventRequest,
/// it is always resolved server-side from ICurrentUserService.
/// CurrentValue is also absent: every new goal starts at 0 (enforced by
/// Goal.Create's default), and for goal types with no computed progress
/// source it is set afterwards through the dedicated
/// PATCH /goals/{id}/progress endpoint, never at creation time.
/// RelatedActivityId is only meaningful for GoalType.ProjectDeadline --
/// see CreateGoalCommandValidator for the field-shape rule and
/// Goal.Create for the domain invariant it mirrors.
/// </summary>
public record CreateGoalRequest(
    GoalType Type,
    decimal TargetValue,
    DateTime? TargetDateUtc,
    Guid? RelatedActivityId);