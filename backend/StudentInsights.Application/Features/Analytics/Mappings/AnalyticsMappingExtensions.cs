using StudentInsights.Application.Common.Academics;
using StudentInsights.Application.Common.Time;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Enums;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Analytics.Mappings;

/// <summary>
/// DTO-shaping logic for the Analytics module. Unlike ExamMappingExtensions/
/// GoalMappingExtensions, these are plain static factory methods rather
/// than "this Entity" extensions: every method here shapes a whole chart
/// (a collection plus already-computed aggregates), not one entity
/// instance, so there is no single natural receiver to extend -- any
/// source entities involved (e.g. the Goals in ToGoalProgressDto, the
/// Courses in ToAcademicAnalyticsDto) are passed as ordinary parameters
/// instead.
///
/// One public method per endpoint, in the same order as
/// AnalyticsController's actions; private helpers used by only one of
/// those methods are grouped at the bottom of the class instead of being
/// interleaved with the public API.
/// </summary>
public static class AnalyticsMappingExtensions
{
    /// <summary>
    /// Builds the Assignment Progress DTO from already-counted totals.
    /// CompletionRatePercentage is guarded against divide-by-zero here --
    /// a user with no LearningActivities yet gets 0%, never NaN or an
    /// exception (see the roadmap's "Common Implementation Mistakes"
    /// section).
    /// </summary>
    public static AssignmentProgressDto ToAssignmentProgressDto(int completed, int pending)
    {
        var total = completed + pending;
        var completionRate = total == 0
            ? 0d
            : Math.Round((double)completed / total * 100, 2);

        var chart = new ChartDatasetDto(
            Labels: new[] { "Completed", "Pending" },
            Datasets: new[]
            {
                new ChartSeriesDto("Assignments", new[] { (double)completed, (double)pending })
            });

        return new AssignmentProgressDto(completed, pending, completionRate, chart);
    }

    /// <summary>
    /// Builds the Goal Progress DTO from goals whose progress has already
    /// been computed via GoalProgressCalculator -- this method does no
    /// progress math of its own, only shaping. A goal whose progress is
    /// NotYetAvailable/NotSupportedYet keeps its real null
    /// ProgressPercentage in GoalProgressItemDto (so callers can tell that
    /// apart from a genuine 0%), but contributes 0 to the chart series,
    /// since a plottable point can't be "unknown".
    /// </summary>
    public static GoalProgressDto ToGoalProgressDto(
        IReadOnlyList<Goal> goals,
        IReadOnlyDictionary<Guid, GoalProgressResult> progressByGoalId)
    {
        var items = goals
            .Select(goal =>
            {
                var progress = progressByGoalId[goal.Id];
                return new GoalProgressItemDto(goal.Id, goal.Type, progress.Status, progress.ProgressPercentage);
            })
            .ToList();

        var chart = new ChartDatasetDto(
            Labels: items.Select(item => item.Type.ToString()).ToList(),
            Datasets: new[]
            {
                new ChartSeriesDto("Progress", items.Select(item => (double)(item.ProgressPercentage ?? 0m)).ToList())
            });

        return new GoalProgressDto(items, chart);
    }

    /// <summary>
    /// Builds the Weekly Activity DTO from LearningActivity completion
    /// timestamps already scoped by the handler to
    /// [weekStartUtc, weekStartUtc + 7 days). Buckets in memory rather
    /// than via a SQL GroupBy: the input is bounded to at most one week's
    /// worth of rows, so this is cheap, and it keeps the day-bucketing
    /// logic in one place instead of pushing an awkward date-truncation
    /// expression into the query -- the same in-memory bucketing approach
    /// Study Time uses for all of its granularities (see ToStudyTimeDto
    /// below).
    /// </summary>
    public static WeeklyActivityDto ToWeeklyActivityDto(
        DateTime weekStartUtc,
        IReadOnlyList<DateTime> completedAtTimestamps)
    {
        const int daysInWeek = 7;

        var labels = new List<string>(daysInWeek);
        for (var i = 0; i < daysInWeek; i++)
            labels.Add(weekStartUtc.AddDays(i).ToString("yyyy-MM-dd"));

        var dailyCounts = new double[daysInWeek];
        foreach (var completedAtUtc in completedAtTimestamps)
        {
            var dayIndex = (completedAtUtc.Date - weekStartUtc.Date).Days;

            // Defensive, not load-bearing: the handler's own query already
            // restricts CompletedAtUtc to this exact week, so dayIndex is
            // always 0..6. Guarding anyway costs nothing and avoids an
            // IndexOutOfRangeException if that invariant is ever loosened.
            if (dayIndex is >= 0 and < daysInWeek)
                dailyCounts[dayIndex]++;
        }

        var chart = new ChartDatasetDto(
            Labels: labels,
            Datasets: new[]
            {
                new ChartSeriesDto("Activities Completed", dailyCounts)
            });

        return new WeeklyActivityDto(weekStartUtc, chart);
    }

    /// <summary>
    /// Builds the Study Time DTO from already-filtered (UserId, optional
    /// From/To) study logs, bucketed at the requested granularity.
    ///
    /// Buckets in memory uniformly across all three granularities, rather
    /// than a SQL GroupBy for Daily/Monthly and only falling back to
    /// in-memory for Weekly: introducing three divergent aggregation
    /// paths for one small chart would be more complex than the data
    /// volume justifies -- a student's study logs, even across several
    /// terms, are the same small-per-user scale the roadmap's own §14
    /// reasoning already relies on for Weekly. This also matches
    /// GetDashboardSummaryQueryHandler's own StudyLogs handling (narrow
    /// projection, then plain LINQ arithmetic), so Study Time doesn't
    /// introduce a second style for the same table.
    ///
    /// Buckets are sparse: only periods with at least one logged session
    /// appear. A dense, zero-filled calendar (every day/week/month in
    /// range, logged or not) was deliberately not built -- doing that
    /// safely for an unbounded "all-time" range (no From/To supplied)
    /// would need its own range-capping logic with no precedent
    /// elsewhere in the project, for a chart that already conveys "no
    /// activity" correctly by simply having no point there.
    /// </summary>
    public static StudyTimeDto ToStudyTimeDto(
        StudyTimeGranularity granularity,
        IReadOnlyList<(DateTime StudyDateUtc, int DurationMinutes)> studyLogs)
    {
        var totalMinutes = studyLogs.Sum(studyLog => studyLog.DurationMinutes);

        var buckets = studyLogs
            .GroupBy(studyLog => GetBucketStart(granularity, studyLog.StudyDateUtc))
            .OrderBy(bucket => bucket.Key)
            .Select(bucket => (
                Label: FormatBucketLabel(granularity, bucket.Key),
                Minutes: (double)bucket.Sum(studyLog => studyLog.DurationMinutes)))
            .ToList();

        var chart = new ChartDatasetDto(
            Labels: buckets.Select(bucket => bucket.Label).ToList(),
            Datasets: new[]
            {
                new ChartSeriesDto("Study Minutes", buckets.Select(bucket => bucket.Minutes).ToList())
            });

        return new StudyTimeDto(granularity, totalMinutes, chart);
    }

    /// <summary>
    /// Builds the (optional, Phase 10.5) Academic Analytics DTO from the
    /// current user's courses. CurrentGpa is computed by
    /// GpaCalculator.CalculateCreditWeighted -- the same shared,
    /// stateless formula GetDashboardSummaryQueryHandler already calls
    /// for its own GPA figure and the Goals module calls for GPA-type
    /// goal progress -- this method does no GPA math of its own, only
    /// shaping. Ungraded courses are excluded from both the averages and
    /// the chart (a course with no FinalGrade yet has nothing to plot),
    /// but are still counted in UngradedCoursesCount so the DTO accounts
    /// for every course, graded or not.
    /// </summary>
    public static AcademicAnalyticsDto ToAcademicAnalyticsDto(IReadOnlyList<Course> courses)
    {
        var gradedCourses = courses.Where(course => course.FinalGrade.HasValue).ToList();

        var currentGpa = GpaCalculator.CalculateCreditWeighted(
            gradedCourses
                .Select(course => ((decimal)course.FinalGrade!.Value, course.Credits))
                .ToList());

        var averageGrade = gradedCourses.Count == 0
            ? (decimal?)null
            : Math.Round(gradedCourses.Average(course => (decimal)course.FinalGrade!.Value), 2);

        var chart = new ChartDatasetDto(
            Labels: gradedCourses.Select(course => course.Name).ToList(),
            Datasets: new[]
            {
                new ChartSeriesDto(
                    "Final Grade",
                    gradedCourses.Select(course => (double)(decimal)course.FinalGrade!.Value).ToList())
            });

        return new AcademicAnalyticsDto(
            currentGpa,
            averageGrade,
            gradedCourses.Count,
            courses.Count - gradedCourses.Count,
            chart);
    }

    /// <summary>
    /// The start of the bucket a given study date falls into, for
    /// ToStudyTimeDto above. Weekly reuses WeekBoundary -- the same
    /// single source of truth Weekly Activity and Dashboard's
    /// WeeklyStudyMinutes already use -- so Study Time's Weekly
    /// bucketing can never disagree with either of them about what
    /// "this week" means.
    /// </summary>
    private static DateTime GetBucketStart(StudyTimeGranularity granularity, DateTime studyDateUtc) =>
        granularity switch
        {
            StudyTimeGranularity.Daily => studyDateUtc.Date,
            StudyTimeGranularity.Weekly => WeekBoundary.GetUtcWeekStart(studyDateUtc),
            StudyTimeGranularity.Monthly => new DateTime(studyDateUtc.Year, studyDateUtc.Month, 1),
            _ => throw new ArgumentOutOfRangeException(
                nameof(granularity), granularity, "Unsupported study time granularity.")
        };

    /// <summary>For ToStudyTimeDto above: yyyy-MM for Monthly, yyyy-MM-dd otherwise.</summary>
    private static string FormatBucketLabel(StudyTimeGranularity granularity, DateTime bucketStart) =>
        granularity == StudyTimeGranularity.Monthly
            ? bucketStart.ToString("yyyy-MM")
            : bucketStart.ToString("yyyy-MM-dd");
}