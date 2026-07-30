using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Academics;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Time;
using StudentInsights.Application.Features.Dashboard.DTOs;
using StudentInsights.Application.Features.Dashboard.Enums;
using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Exams.Mappings;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.DTOs;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Composes independent, user-scoped reads across Courses, Exams,
/// LearningActivities, Goals, StudyLogs, and Notifications into a single
/// aggregate payload. Mirrors GetCalendarEventsQueryHandler's shape:
/// queries run sequentially against the one scoped IApplicationDbContext
/// rather than via Task.WhenAll, since DbContext is not safe for
/// concurrent use. Dashboard owns no entity and issues no writes -- this
/// handler is the entirety of the module's logic, same as Calendar.
/// </summary>
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private const int UpcomingExamsLimit = 5;
    private const int RecentActivityCandidatesPerSource = 5;
    private const int RecentActivitiesLimit = 5;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var utcNow = DateTime.UtcNow;

        // A student's course list is small and bounded by a term (unlike
        // StudyLogs), so one fetch cheaply serves three independent views:
        // the total count, the GPA input, and the "recent courses" slice
        // of RecentActivities -- three round trips would just re-read the
        // same handful of rows three times.
        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

        var totalCourses = courses.Count;

        // Scoped to the user's current semester (see
        // CurrentSemesterCourseProvider's remarks for what "current" means)
        // rather than a lifetime average across every graded course the
        // user has ever entered -- same scoping GoalProgressInputsProvider
        // now applies to a GPA-type goal's progress. Computed from the
        // `courses` list already loaded above, so this costs no extra query.
        var creditWeightedGpa = CurrentSemesterCourseProvider.GetCurrentSemesterGpa(courses);

        var (activeAssignmentsCount, activeProjectsCount) = await GetActiveActivityCountsAsync(userId, cancellationToken);

        // Same reasoning as Courses: a term's exams are few enough that
        // fetching all of them once (with the Course include ExamDto
        // needs anyway) is cheaper than a separate "upcoming" and
        // "recent" round trip. If exams are ever kept across many terms
        // without archiving, this should revert to two bounded queries.
        var exams = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken);

        var upcomingExams = exams
            .Where(e => e.ExamDateUtc >= utcNow)
            .OrderBy(e => e.ExamDateUtc)
            .Take(UpcomingExamsLimit)
            .Select(e => e.ToDto())
            .ToList();

        var goals = await _context.Goals
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .ToListAsync(cancellationToken);

        var relatedActivityStatuses = await GetRelatedActivityStatusesAsync(userId, goals, cancellationToken);

        var weekStartUtc = WeekBoundary.GetUtcWeekStart(utcNow);
        var studyLogs = await GetStudyLogsAsync(userId, goals, weekStartUtc, cancellationToken);

        var weeklyStudyMinutes = studyLogs
            .Where(sl => sl.StudyDateUtc >= weekStartUtc)
            .Sum(sl => sl.DurationMinutes);

        var goalsProgress = goals
            .Select(goal =>
            {
                var inputs = GoalProgressInputsProvider.BuildInputs(
                    goal,
                    creditWeightedGpa,
                    studyLogs,
                    relatedActivityStatuses);

                var progress = GoalProgressCalculator.CalculateProgress(goal, inputs);

                return goal.ToDto(progress);
            })
            .ToList();

        var recentLearningActivities = await GetRecentLearningActivitiesAsync(userId, cancellationToken);
        var recentActivities = BuildRecentActivities(courses, exams, recentLearningActivities);

        var unreadNotificationsCount = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);

        return new DashboardSummaryDto(
            totalCourses,
            activeAssignmentsCount,
            activeProjectsCount,
            upcomingExams,
            goalsProgress,
            weeklyStudyMinutes,
            recentActivities,
            unreadNotificationsCount);
    }

    private async Task<(int ActiveAssignmentsCount, int ActiveProjectsCount)> GetActiveActivityCountsAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        // Kept as a dedicated grouped-COUNT query, deliberately NOT merged
        // with the "recent activities" read below: unlike Courses/Exams, a
        // term's LearningActivities can run into the hundreds (weekly
        // homework across many courses), so letting SQL do the aggregation
        // and the separate top-N selection scales better than
        // materializing the whole table on every dashboard load.
        var activeCountsByType = await _context.LearningActivities
            .AsNoTracking()
            .Where(la => la.UserId == userId && la.Status != ActivityStatus.Completed)
            .GroupBy(la => la.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeAssignmentsCount = activeCountsByType.FirstOrDefault(x => x.Type == ActivityType.Assignment)?.Count ?? 0;
        var activeProjectsCount = activeCountsByType.FirstOrDefault(x => x.Type == ActivityType.Project)?.Count ?? 0;

        return (activeAssignmentsCount, activeProjectsCount);
    }

    private async Task<IReadOnlyDictionary<Guid, ActivityStatus>> GetRelatedActivityStatusesAsync(
        Guid userId, IReadOnlyList<Goal> goals, CancellationToken cancellationToken)
    {
        var relatedActivityIds = goals
            .Where(g => g.Type == GoalType.ProjectDeadline && g.RelatedActivityId.HasValue)
            .Select(g => g.RelatedActivityId!.Value)
            .Distinct()
            .ToList();

        if (relatedActivityIds.Count == 0)
            return new Dictionary<Guid, ActivityStatus>();

        // UserId is filtered here too, even though Goal.Create already
        // guarantees a ProjectDeadline goal's RelatedActivityId belongs to
        // the same user -- every query in this handler independently
        // enforces ownership rather than relying on another aggregate's
        // invariant, per the project's own ownership-isolation standard.
        return await _context.LearningActivities
            .AsNoTracking()
            .Where(la => la.UserId == userId && relatedActivityIds.Contains(la.Id))
            .ToDictionaryAsync(la => la.Id, la => la.Status, cancellationToken);
    }

    private async Task<IReadOnlyList<(DateTime StudyDateUtc, int DurationMinutes)>> GetStudyLogsAsync(
        Guid userId,
        IReadOnlyList<Goal> goals,
        DateTime weekStartUtc,
        CancellationToken cancellationToken)
    {
        // Bounded, not "all logs ever": a StudyHours goal's progress needs
        // logs back to that goal's own CreatedAtUtc, so the fetch only
        // needs to reach as far back as the earliest such goal (or the
        // current week's start, whichever is earlier).
        var earliestNeededUtc = goals
            .Where(g => g.Type == GoalType.StudyHours)
            .Select(g => g.CreatedAtUtc)
            .Append(weekStartUtc)
            .Min();

        return await _context.StudyLogs
            .AsNoTracking()
            .Where(sl => sl.UserId == userId && sl.StudyDateUtc >= earliestNeededUtc)
            .Select(sl => new ValueTuple<DateTime, int>(
                sl.StudyDateUtc,
                sl.DurationMinutes))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RecentActivityDto>> GetRecentLearningActivitiesAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        return await _context.LearningActivities
            .AsNoTracking()
            .Where(la => la.UserId == userId)
            .OrderByDescending(la => la.UpdatedAtUtc ?? la.CreatedAtUtc)
            .Take(RecentActivityCandidatesPerSource)
            .Select(la => new RecentActivityDto(la.Id, RecentActivitySourceType.LearningActivity, la.Title, la.UpdatedAtUtc ?? la.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<RecentActivityDto> BuildRecentActivities(
        IReadOnlyList<Course> courses,
        IReadOnlyList<Exam> exams,
        IReadOnlyList<RecentActivityDto> recentLearningActivities)
    {
        var recentCourses = courses
            .OrderByDescending(c => c.UpdatedAtUtc ?? c.CreatedAtUtc)
            .Take(RecentActivityCandidatesPerSource)
            .Select(c => new RecentActivityDto(c.Id, RecentActivitySourceType.Course, c.Name, c.UpdatedAtUtc ?? c.CreatedAtUtc));

        var recentExams = exams
            .OrderByDescending(e => e.UpdatedAtUtc ?? e.CreatedAtUtc)
            .Take(RecentActivityCandidatesPerSource)
            .Select(e => new RecentActivityDto(e.Id, RecentActivitySourceType.Exam, e.Title, e.UpdatedAtUtc ?? e.CreatedAtUtc));

        // Single merge-and-sort after all three sources are ready, not
        // per-source. Id is a deterministic (not meaningfully "stable",
        // but reproducible) tie-break for the identical-timestamp edge
        // case the roadmap calls out.
        return recentCourses
            .Concat(recentExams)
            .Concat(recentLearningActivities)
            .OrderByDescending(a => a.OccurredAtUtc)
            .ThenByDescending(a => a.Id)
            .Take(RecentActivitiesLimit)
            .ToList();
    }

}