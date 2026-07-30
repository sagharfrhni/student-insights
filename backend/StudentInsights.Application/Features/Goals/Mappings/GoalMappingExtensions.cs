using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Goals.Mappings;

public static class GoalMappingExtensions
{
    public static GoalDto ToDto(this Goal goal, GoalProgressResult progress)
    {
        return new GoalDto(
            goal.Id,
            goal.Type,
            goal.TargetValue,
            goal.CurrentValue,
            goal.TargetDateUtc,
            goal.RelatedActivityId,
            progress.Status,
            progress.ProgressPercentage,
            goal.CreatedAtUtc,
            goal.UpdatedAtUtc);
    }
}