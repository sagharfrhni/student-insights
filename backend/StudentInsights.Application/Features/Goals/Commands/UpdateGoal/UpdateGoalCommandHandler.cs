using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;

namespace StudentInsights.Application.Features.Goals.Commands.UpdateGoal;

public class UpdateGoalCommandHandler : IRequestHandler<UpdateGoalCommand, GoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateGoalCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GoalDto> Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == request.GoalId, cancellationToken);

        // Same 404-for-both-cases reasoning as UpdateCourseCommandHandler.
        if (goal is null || goal.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Goal '{request.GoalId}' was not found.");

        goal.UpdateTarget(request.TargetValue, request.TargetDateUtc);

        await _context.SaveChangesAsync(cancellationToken);

        var progressInputs = await GoalProgressInputsProvider.GetAsync(_context, goal.UserId, goal, cancellationToken);
        var progress = GoalProgressCalculator.CalculateProgress(goal, progressInputs);

        return goal.ToDto(progress);
    }
}