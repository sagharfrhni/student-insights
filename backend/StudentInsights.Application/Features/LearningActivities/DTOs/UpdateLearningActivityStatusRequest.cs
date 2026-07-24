using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.LearningActivities.DTOs;

/// <summary>
/// User-supplied payload for the dedicated status-change endpoint
/// (PATCH /{id}/status), mirroring the narrow, single-purpose PATCH
/// /{id}/grade shape used elsewhere in the product spec. Kept separate
/// from UpdateLearningActivityRequest so a full-details edit can never
/// accidentally smuggle in a status change, and vice versa.
/// </summary>
public record UpdateLearningActivityStatusRequest(ActivityStatus NewStatus);