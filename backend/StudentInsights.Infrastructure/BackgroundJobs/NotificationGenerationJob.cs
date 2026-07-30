using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Goals.Common;
using StudentInsights.Application.Features.Goals.Enums;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Application.Features.Notifications.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Infrastructure.BackgroundJobs;

/// <summary>
/// Recurring, system-triggered scan that turns silent deadline/exam/goal
/// states in other modules into Notification rows. Registered as a
/// single hourly Hangfire recurring job (see Program.cs) rather than one
/// job per check, so the "all four run together, hourly" contract lives
/// in exactly one place.
///
/// Deliberately a plain class depending only on IApplicationDbContext/
/// ILogger — no Hangfire.* types are referenced here. Hangfire's own
/// DI-based job activator (wired via AddHangfire in Program.cs) resolves
/// this constructor the same way ASP.NET Core resolves a controller — no
/// explicit service registration is needed for this class itself.
/// Keeping it framework-agnostic also means it can be unit-tested like
/// any other class in this project, with no Hangfire test harness
/// required.
///
/// Two layers of failure isolation, each guarding a different failure
/// surface:
/// - RunCheckAsync (check-level): guards each check's upfront candidate
///   query (e.g. the Exams/LearningActivities read), so one check
///   failing outright never prevents the other three from running.
/// - TryCreateNotificationSafeAsync (candidate-level): guards each
///   individual NotificationFactory.TryCreateAsync call inside a check's
///   loop, so one bad candidate (e.g. a race where a user is deleted
///   between GetActiveUserIdsAsync and this candidate being processed)
///   is logged and skipped instead of throwing past SaveChangesAsync and
///   silently discarding every notification already staged earlier in
///   that same loop, for every other user.
/// The Goal check additionally wraps each user-group's processing in its
/// own try/catch, since it alone runs an extra query
/// (GoalProgressInputsProvider.GetBatchAsync) inside its loop that the
/// other three checks don't have.
/// </summary>
public class NotificationGenerationJob
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationGenerationJob> _logger;

    public NotificationGenerationJob(IApplicationDbContext context, ILogger<NotificationGenerationJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Entry point Hangfire calls (see the "notification-generation" recurring job registered in Program.cs).</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var activeUserIds = await GetActiveUserIdsAsync(cancellationToken);

        var examCount = await RunCheckAsync(
            "ExamTomorrow",
            () => GenerateExamTomorrowAsync(activeUserIds, utcNow, cancellationToken));

        var deadlineCount = await RunCheckAsync(
            "DeadlineApproaching",
            () => GenerateDeadlineApproachingAsync(activeUserIds, utcNow, cancellationToken));

        var overdueCount = await RunCheckAsync(
            "OverdueActivity",
            () => GenerateOverdueActivityAsync(activeUserIds, utcNow, cancellationToken));

        var goalCount = await RunCheckAsync(
            "GoalBehindSchedule",
            () => GenerateGoalBehindScheduleAsync(activeUserIds, utcNow, cancellationToken));

        _logger.LogInformation(
            "Notification generation run complete: ExamTomorrow={ExamTomorrow}, DeadlineApproaching={DeadlineApproaching}, OverdueActivity={OverdueActivity}, GoalBehindSchedule={GoalBehindSchedule} ({Total} total)",
            examCount, deadlineCount, overdueCount, goalCount,
            examCount + deadlineCount + overdueCount + goalCount);
    }

    /// <summary>
    /// Runs one check's upfront work (its candidate query and loop),
    /// logging and swallowing any exception it throws so the other three
    /// checks in the same run are unaffected — per the module roadmap,
    /// §23. Deliberately narrow: only wraps the check itself, not
    /// GetActiveUserIdsAsync above, so a genuinely broken run (e.g. the
    /// database is unreachable) still propagates out of RunAsync for
    /// Hangfire's own retry policy to handle.
    /// </summary>
    private async Task<int> RunCheckAsync(string checkName, Func<Task<int>> check)
    {
        try
        {
            return await check();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification generation check '{Check}' failed; other checks were unaffected.", checkName);
            return 0;
        }
    }

    /// <summary>
    /// Wraps a single NotificationFactory.TryCreateAsync call so one bad
    /// candidate can't throw past the end of its check's loop and cause
    /// SaveChangesAsync to be skipped for every notification already
    /// staged earlier in that same loop. Logs with the specific
    /// user/type/source that failed — more precise than the check-level
    /// log in RunCheckAsync, since it identifies exactly which candidate
    /// caused the problem.
    /// </summary>
    private async Task<bool> TryCreateNotificationSafeAsync(
        Guid userId,
        NotificationType type,
        string message,
        Guid sourceId,
        CancellationToken cancellationToken,
        DateTime? ignoreExistingBeforeUtc = null)
    {
        try
        {
            var notification = await NotificationFactory.TryCreateAsync(
                _context, userId, type, message, sourceId, cancellationToken, ignoreExistingBeforeUtc);

            return notification is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to create a '{Type}' notification for user '{UserId}' (source '{SourceId}'); skipping this candidate.",
                type, userId, sourceId);
            return false;
        }
    }

    /// <summary>
    /// Only active users receive generated notifications — deactivated
    /// accounts (User.IsActive == false) are skipped entirely, per §20.
    /// Computed once and reused as an IN (...) filter across all four
    /// checks below, avoiding a per-check "is this user active" query.
    /// </summary>
    private async Task<List<Guid>> GetActiveUserIdsAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Source: Exam. Notifies once per exam whose ExamDateUtc falls on
    /// the UTC calendar day immediately following "now" — see §16. No
    /// Status/completion concept exists on Exam, so every exam in the
    /// window qualifies; duplicate prevention (one notification per
    /// exam, ever) is entirely NotificationFactory's responsibility.
    /// </summary>
    private async Task<int> GenerateExamTomorrowAsync(
        IReadOnlyCollection<Guid> activeUserIds, DateTime utcNow, CancellationToken cancellationToken)
    {
        var tomorrowStartUtc = utcNow.Date.AddDays(1);
        var tomorrowEndUtc = tomorrowStartUtc.AddDays(1);

        var candidateExams = await _context.Exams
            .AsNoTracking()
            .Where(e => activeUserIds.Contains(e.UserId)
                && e.ExamDateUtc >= tomorrowStartUtc
                && e.ExamDateUtc < tomorrowEndUtc)
            .Select(e => new { e.Id, e.UserId, e.Title })
            .ToListAsync(cancellationToken);

        var createdCount = 0;

        foreach (var exam in candidateExams)
        {
            var created = await TryCreateNotificationSafeAsync(
                exam.UserId,
                NotificationType.ExamTomorrow,
                $"You have an exam tomorrow: '{exam.Title}'.",
                exam.Id,
                cancellationToken);

            if (created)
                createdCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdCount;
    }

    /// <summary>
    /// Source: LearningActivity. Notifies once per activity whose
    /// DueDateUtc falls within the next 24 hours and which is not yet
    /// Completed — §15. Duplicate prevention (one notification per
    /// activity, ever) is entirely NotificationFactory's responsibility.
    /// </summary>
    private async Task<int> GenerateDeadlineApproachingAsync(
        IReadOnlyCollection<Guid> activeUserIds, DateTime utcNow, CancellationToken cancellationToken)
    {
        var deadlineWindowEndUtc = utcNow.AddHours(24);

        var candidateActivities = await _context.LearningActivities
            .AsNoTracking()
            .Where(la => activeUserIds.Contains(la.UserId)
                && la.DueDateUtc >= utcNow
                && la.DueDateUtc <= deadlineWindowEndUtc
                && la.Status != ActivityStatus.Completed)
            .Select(la => new { la.Id, la.UserId, la.Title })
            .ToListAsync(cancellationToken);

        var createdCount = 0;

        foreach (var activity in candidateActivities)
        {
            var created = await TryCreateNotificationSafeAsync(
                activity.UserId,
                NotificationType.DeadlineApproaching,
                $"'{activity.Title}' is due soon.",
                activity.Id,
                cancellationToken);

            if (created)
                createdCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdCount;
    }

    /// <summary>
    /// Source: LearningActivity (both Assignment and Project — the
    /// roadmap's "Overdue Assignment" naming maps to the same
    /// LearningActivity entity the check above uses; see §17). Notifies
    /// once per activity that is currently overdue and not yet
    /// Completed.
    ///
    /// Fully implements the §19 reopen refinement: if the activity was
    /// previously completed, reopened, and is overdue again,
    /// LastCompletedAtUtc (set on Complete(), never cleared by Reopen() —
    /// see LearningActivity) is passed as ignoreExistingBeforeUtc, so an
    /// OverdueActivity notification created *before* that completion no
    /// longer blocks a fresh one. A brand-new, never-completed overdue
    /// activity has LastCompletedAtUtc == null, which preserves the base
    /// "one notification ever" rule.
    /// </summary>
    private async Task<int> GenerateOverdueActivityAsync(
        IReadOnlyCollection<Guid> activeUserIds, DateTime utcNow, CancellationToken cancellationToken)
    {
        var candidateActivities = await _context.LearningActivities
            .AsNoTracking()
            .Where(la => activeUserIds.Contains(la.UserId)
                && la.DueDateUtc < utcNow
                && la.Status != ActivityStatus.Completed)
            .Select(la => new { la.Id, la.UserId, la.Title, la.LastCompletedAtUtc })
            .ToListAsync(cancellationToken);

        var createdCount = 0;

        foreach (var activity in candidateActivities)
        {
            var created = await TryCreateNotificationSafeAsync(
                activity.UserId,
                NotificationType.OverdueActivity,
                $"'{activity.Title}' is overdue.",
                activity.Id,
                cancellationToken,
                ignoreExistingBeforeUtc: activity.LastCompletedAtUtc);

            if (created)
                createdCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdCount;
    }

    /// <summary>
    /// Source: Goal, via the existing GoalProgressCalculator/
    /// GoalProgressInputsProvider — this check does not invent a new
    /// definition of progress (§3, §18). Only goals with a TargetDateUtc
    /// have a time axis to fall behind on; goals without one are
    /// excluded by the query below. Processed per user because
    /// GoalProgressInputsProvider.GetBatchAsync is itself scoped to one
    /// userId (its GPA/StudyLog queries are Where(UserId == userId)) —
    /// same reasoning GetGoalsQueryHandler and the module roadmap give
    /// for not batching across users.
    ///
    /// Each user's group is wrapped in its own try/catch: unlike the
    /// other three checks, this one runs an extra query
    /// (GetBatchAsync) inside the loop, so one user's data causing that
    /// query to fail should not prevent every other user's goals in the
    /// same run from being evaluated.
    /// </summary>
    private async Task<int> GenerateGoalBehindScheduleAsync(
        IReadOnlyCollection<Guid> activeUserIds, DateTime utcNow, CancellationToken cancellationToken)
    {
        var candidateGoals = await _context.Goals
            .AsNoTracking()
            .Where(g => activeUserIds.Contains(g.UserId) && g.TargetDateUtc != null)
            .ToListAsync(cancellationToken);

        var createdCount = 0;

        foreach (var goalsForUser in candidateGoals.GroupBy(g => g.UserId))
        {
            try
            {
                var progressByGoalId = await GoalProgressInputsProvider.GetBatchAsync(
                    _context, goalsForUser.Key, goalsForUser.ToList(), cancellationToken);

                foreach (var goal in goalsForUser)
                {
                    var progress = GoalProgressCalculator.CalculateProgress(goal, progressByGoalId[goal.Id]);

                    if (!IsBehindSchedule(goal, progress, utcNow))
                        continue;

                    var created = await TryCreateNotificationSafeAsync(
                        goal.UserId,
                        NotificationType.GoalBehindSchedule,
                        "You're falling behind on your goal.",
                        goal.Id,
                        cancellationToken);

                    if (created)
                        createdCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to evaluate goal progress for user '{UserId}'; skipping this user for this run.",
                    goalsForUser.Key);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdCount;
    }

    /// <summary>
    /// A goal is behind schedule when the fraction of time elapsed since
    /// its creation exceeds the fraction of progress achieved, per §18.
    /// Goals whose deadline has already passed are deliberately excluded
    /// (elapsedFraction &lt; 1.0 guard) — a goal missed entirely is a
    /// different, not-yet-modeled state per the roadmap's own note, not
    /// "behind schedule".
    /// </summary>
    private static bool IsBehindSchedule(Goal goal, GoalProgressResult progress, DateTime utcNow)
    {
        if (progress.Status != GoalProgressStatus.Available || progress.ProgressPercentage is not { } progressPercentage)
            return false;

        // TargetDateUtc is guaranteed non-null by the caller's query filter.
        var totalSpanSeconds = (goal.TargetDateUtc!.Value - goal.CreatedAtUtc).TotalSeconds;
        if (totalSpanSeconds <= 0)
            return false; // Malformed data (target at/before creation) — nothing meaningful to compare.

        var elapsedFraction = (utcNow - goal.CreatedAtUtc).TotalSeconds / totalSpanSeconds;
        var progressFraction = (double)progressPercentage / 100.0;

        return elapsedFraction > progressFraction && elapsedFraction < 1.0;
    }
}