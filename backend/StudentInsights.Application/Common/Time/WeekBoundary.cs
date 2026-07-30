namespace StudentInsights.Application.Common.Time;

/// <summary>
/// Single source of truth for "when does a week start" across the
/// application. Isolated here -- not inlined into any one feature's
/// handler -- so that a future need for per-locale or configurable week
/// boundaries changes only this file; callers never need to know how the
/// boundary is determined, only that "this week" means whatever
/// GetUtcWeekStart returns.
/// </summary>
public static class WeekBoundary
{
    /// <summary>
    /// The first day of a calendar week for this product's locale.
    /// StudentInsights targets Persian-speaking users in Iran, where the
    /// week starts on Saturday.
    /// </summary>
    public const DayOfWeek WeekStartDay = DayOfWeek.Saturday;

    /// <summary>
    /// Returns the UTC start-of-day for the week containing utcNow, using
    /// WeekStartDay as the first day of the week. utcNow is expected to
    /// already be in UTC, consistent with every other *Utc comparison in
    /// the project.
    /// </summary>
    public static DateTime GetUtcWeekStart(DateTime utcNow)
    {
        var daysSinceWeekStart = ((int)utcNow.DayOfWeek - (int)WeekStartDay + 7) % 7;
        return utcNow.Date.AddDays(-daysSinceWeekStart);
    }
}