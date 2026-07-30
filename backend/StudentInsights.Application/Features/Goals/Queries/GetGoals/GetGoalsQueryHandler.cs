using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Goals.Queries.GetGoals;

public class GetGoalsQueryHandler : IRequestHandler<GetGoalsQuery, PaginatedResult<GoalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGoalsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<GoalDto>> Handle(GetGoalsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Goals
            .AsNoTracking()
            .Where(g => g.UserId == _currentUserService.UserId)
            .OrderByDescending(g => g.CreatedAtUtc);

        var pagedGoals = await PaginatedResult<Goal>.CreateAsync(
            query,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);

        // Progress can't be computed inside the IQueryable pipeline (it's
        // business logic, not translatable to SQL -- same reasoning as
        // GetCoursesQueryHandler's Course.ToDto()), so paging happens on
        // IQueryable<Goal> above and progress is computed only on the
        // already-materialized page, batched to avoid N+1 across it.
        var progressByGoalId = await GoalProgressInputsProvider.GetBatchAsync(
            _context, _currentUserService.UserId, pagedGoals.Items, cancellationToken);

        return pagedGoals.Map(goal =>
        {
            var progress = GoalProgressCalculator.CalculateProgress(goal, progressByGoalId[goal.Id]);
            return goal.ToDto(progress);
        });
    }
}