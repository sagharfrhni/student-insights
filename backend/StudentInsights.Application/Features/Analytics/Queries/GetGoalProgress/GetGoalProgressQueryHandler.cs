using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Mappings;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.Services;

namespace StudentInsights.Application.Features.Analytics.Queries.GetGoalProgress;

public class GetGoalProgressQueryHandler : IRequestHandler<GetGoalProgressQuery, GoalProgressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGoalProgressQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GoalProgressDto> Handle(GetGoalProgressQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // Newest first, same ordering GetGoalsQueryHandler already uses --
        // Analytics' Goal Progress is the chart counterpart of that same
        // list, so it should present goals in the same order.
        var goals = await _context.Goals
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // Batched, not one query per goal -- at most three extra queries
        // total regardless of how many goals the user has (see
        // GoalProgressInputsProvider's own remarks). The progress formula
        // itself lives entirely in GoalProgressCalculator; Analytics must
        // never reimplement it (roadmap §21's top-listed mistake).
        var inputsByGoalId = await GoalProgressInputsProvider.GetBatchAsync(
            _context, userId, goals, cancellationToken);

        var progressByGoalId = goals.ToDictionary(
            goal => goal.Id,
            goal => GoalProgressCalculator.CalculateProgress(goal, inputsByGoalId[goal.Id]));

        return AnalyticsMappingExtensions.ToGoalProgressDto(goals, progressByGoalId);
    }
}