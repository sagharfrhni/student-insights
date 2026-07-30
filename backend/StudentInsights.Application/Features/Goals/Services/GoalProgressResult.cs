using StudentInsights.Application.Features.Goals.Enums;

namespace StudentInsights.Application.Features.Goals.Services;

public record GoalProgressResult(GoalProgressStatus Status, decimal? ProgressPercentage);