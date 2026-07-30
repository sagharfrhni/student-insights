using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.Commands.CreateGoal;

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateGoalCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GoalDto> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User '{_currentUserService.UserId}' was not found.");

        // GPA is a single, live-computed number (GpaCalculator), so a second
        // active GradePointAverage goal would just be a duplicate target for
        // the same figure. Backstopped by GoalConfiguration's matching
        // unique filtered index. StudyHours/ChapterCount are intentionally
        // NOT restricted here — nothing in the current model ties either to
        // a single source, so a user may legitimately want more than one at
        // once (e.g. two different textbooks' chapter counts).
        if (request.Type == GoalType.GradePointAverage)
        {
            var hasActiveGpaGoal = await _context.Goals
                .AnyAsync(g => g.UserId == user.Id && g.Type == GoalType.GradePointAverage, cancellationToken);

            if (hasActiveGpaGoal)
                throw new DomainException(
                    "You already have an active GPA goal. Update its target instead of creating a new one.");
        }

        LearningActivity? relatedActivity = null;
        if (request.RelatedActivityId is not null)
        {
            relatedActivity = await _context.LearningActivities
                .FirstOrDefaultAsync(la => la.Id == request.RelatedActivityId, cancellationToken);

            if (relatedActivity is null || relatedActivity.UserId != _currentUserService.UserId)
                throw new NotFoundException($"Learning activity '{request.RelatedActivityId}' was not found.");

            // A ProjectDeadline goal is keyed by which activity it tracks,
            // so a second one pointing at the same activity would be a
            // redundant duplicate. Backstopped by GoalConfiguration's
            // matching unique filtered index.
            var hasGoalForActivity = await _context.Goals
                .AnyAsync(g => g.UserId == user.Id && g.RelatedActivityId == relatedActivity.Id, cancellationToken);

            if (hasGoalForActivity)
                throw new DomainException(
                    "A project-deadline goal for this learning activity already exists.");
        }

        var goal = Goal.Create(user, request.Type, request.TargetValue, request.TargetDateUtc, relatedActivity);

        _context.Goals.Add(goal);

        await _context.SaveChangesAsync(cancellationToken);

        var progressInputs = await GoalProgressInputsProvider.GetAsync(_context, user.Id, goal, cancellationToken);
        var progress = GoalProgressCalculator.CalculateProgress(goal, progressInputs);

        return goal.ToDto(progress);
    }
}