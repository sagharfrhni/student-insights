using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Goals.Commands.UpdateGoalProgress;

/// <summary>
/// The only path allowed to move Goal.CurrentValue. Restricted to goal
/// types GoalProgressCalculator.IsManuallyTracked reports as true --
/// letting this succeed for a computed type (e.g. GradePointAverage)
/// would write a value GoalProgressCalculator never reads back, silently
/// going stale. See UpdateGoalProgressCommandValidator for why that check
/// lives here instead: it depends on the loaded entity's Type, a
/// stateful rule the validator can't see.
/// </summary>
public class UpdateGoalProgressCommandHandler : IRequestHandler<UpdateGoalProgressCommand, GoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateGoalProgressCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GoalDto> Handle(UpdateGoalProgressCommand request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == request.GoalId, cancellationToken);

        // Same 404-for-both-cases reasoning as UpdateCourseCommandHandler.
        if (goal is null || goal.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Goal '{request.GoalId}' was not found.");

        if (!GoalProgressCalculator.IsManuallyTracked(goal.Type))
            throw new DomainException(
                $"Progress for a '{goal.Type}' goal is calculated automatically and cannot be updated manually.");

        goal.UpdateProgress(request.CurrentValue);

        await _context.SaveChangesAsync(cancellationToken);

        var progressInputs = await GoalProgressInputsProvider.GetAsync(_context, goal.UserId, goal, cancellationToken);
        var progress = GoalProgressCalculator.CalculateProgress(goal, progressInputs);

        return goal.ToDto(progress);
    }
}