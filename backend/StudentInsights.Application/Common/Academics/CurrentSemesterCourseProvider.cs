using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Common.Academics;

/// <summary>
/// Resolves which of a user's courses count as "the current semester" for
/// GPA purposes, and returns the credit-weighted GPA over just those
/// courses. Centralized here — rather than inlined into
/// GoalProgressInputsProvider — so GetDashboardSummaryQueryHandler's
/// separate Courses query (which, per GoalProgressInputsProvider's own
/// documented contract, must resolve GPA-goal progress exactly the same
/// way GetBatchAsync does) can call this identical logic instead of
/// re-deriving "what counts as the current semester" a second time and
/// risking the two features silently disagreeing on a student's GPA.
/// GetAcademicAnalyticsQueryHandler reuses the same resolution (via
/// GetCurrentSemesterCourses) for the same reason.
///
/// "Current semester" is deliberately NOT a calendar concept: Course has no
/// start/end date, only a free-text Semester label (see Course.Semester),
/// and there is no separate "active semester" setting anywhere in the
/// system. Instead, it's defined as whichever Semester value belongs to
/// the user's most recently created course — the term the student is
/// actually entering data for right now. This is the smallest rule that
/// closes the gap the product-readiness review raised (GPA silently
/// blending every semester the student has ever entered) without
/// requiring a new concept (an "active term" setting, a calendar range,
/// etc.) that nothing else in the product currently needs.
/// </summary>
public static class CurrentSemesterCourseProvider
{
    /// <summary>
    /// The subset of the given courses that belong to the user's current
    /// semester (see class remarks for what "current" means). Empty (not
    /// null) when the input is empty. This is the one place "what counts
    /// as the current semester" is decided -- GetCurrentSemesterGpa and
    /// GetAcademicAnalyticsQueryHandler both filter through here so a
    /// student's GPA, goal progress, and analytics screen can never
    /// disagree on which courses are "current".
    /// </summary>
    public static IReadOnlyList<Course> GetCurrentSemesterCourses(IReadOnlyList<Course> courses)
    {
        // ThenByDescending(Id) is a deterministic tie-break for the rare
        // case where two courses share an identical CreatedAtUtc -- same
        // reasoning and pattern as BuildRecentActivities' identical-
        // timestamp tie-break in GetDashboardSummaryQueryHandler.
        var currentSemester = courses
            .OrderByDescending(c => c.CreatedAtUtc)
            .ThenByDescending(c => c.Id)
            .Select(c => c.Semester)
            .FirstOrDefault();

        // No courses at all yet.
        if (currentSemester is null)
            return Array.Empty<Course>();

        return courses.Where(c => c.Semester == currentSemester).ToList();
    }

    /// <summary>
    /// Credit-weighted GPA over the graded courses in the user's current
    /// semester only, computed from an already-loaded list of the user's
    /// courses. Use this overload when the caller has already fetched its
    /// courses for another reason (e.g. GetDashboardSummaryQueryHandler,
    /// which loads every course for the total-count and recent-activity
    /// views anyway) so GPA scoping doesn't cost a second round trip. Null
    /// when the list is empty, or has no graded course within the current
    /// semester — same "not yet available" contract as
    /// GpaCalculator.CalculateCreditWeighted.
    /// </summary>
    public static decimal? GetCurrentSemesterGpa(IReadOnlyList<Course> courses)
    {
        var gradedCourses = GetCurrentSemesterCourses(courses)
            .Where(c => c.FinalGrade != null)
            .Select(c => ((decimal)c.FinalGrade!.Value, c.Credits))
            .ToList();

        return GpaCalculator.CalculateCreditWeighted(gradedCourses);
    }

    /// <summary>
    /// Same result as GetCurrentSemesterGpa, for callers (e.g.
    /// GoalProgressInputsProvider) that don't already have the user's
    /// courses loaded and only want them for this one calculation.
    /// Delegates to GetCurrentSemesterGpa once fetched, rather than
    /// re-implementing the "current semester" rule a second time, so the
    /// two overloads can never drift apart.
    /// </summary>
    public static async Task<decimal?> GetCurrentSemesterGpaAsync(
        IApplicationDbContext context, Guid userId, CancellationToken cancellationToken)
    {
        var courses = await context.Courses
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

        return GetCurrentSemesterGpa(courses);
    }
}
