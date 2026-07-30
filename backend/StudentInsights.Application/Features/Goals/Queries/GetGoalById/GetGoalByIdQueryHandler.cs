using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;

namespace StudentInsights.Application.Features.Goals.Queries.GetGoalById;

public class GetGoalByIdQueryHandler : IRequestHandler<GetGoalByIdQuery, GoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGoalByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GoalDto> Handle(GetGoalByIdQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: this is a pure read, the entity is never mutated
        // or saved, so there's no reason to pay for EF's change tracking.
        var goal = await _context.Goals
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == request.GoalId, cancellationToken);

        // Same 404-for-both-cases reasoning as GetCourseByIdQueryHandler:
        // don't let a 403 confirm that a GoalId belonging to someone else
        // exists.
        if (goal is null || goal.UserId != _currentUserService.UserId)
            throw new NotFoundException($"Goal '{request.GoalId}' was not found.");

        var progressInputs = await GoalProgressInputsProvider.GetAsync(_context, goal.UserId, goal, cancellationToken);
        var progress = GoalProgressCalculator.CalculateProgress(goal, progressInputs);

        return goal.ToDto(progress);
    }
}