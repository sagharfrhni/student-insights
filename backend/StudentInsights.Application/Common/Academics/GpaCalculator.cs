namespace StudentInsights.Application.Common.Academics;

/// <summary>
/// Credit-weighted GPA calculation. Shared, stateless, no I/O -- callers
/// fetch the graded courses; this only does the arithmetic. Not
/// Goals-specific: a future Courses or Analytics feature (per the
/// product vision's academic-performance-analysis view) will want the
/// same number without depending on Goals to get it.
/// </summary>
public static class GpaCalculator
{
    /// <summary>
    /// Credit-weighted average over courses that have a FinalGrade set.
    /// Returns null when there are no graded courses (or, defensively,
    /// zero total credits) -- "not yet available" is left for the caller
    /// to decide how to present, not baked in here as a fake 0.
    /// </summary>
    public static decimal? CalculateCreditWeighted(
        IReadOnlyCollection<(decimal FinalGrade, int Credits)> gradedCourses)
    {
        if (gradedCourses.Count == 0)
            return null;

        var totalCredits = gradedCourses.Sum(c => c.Credits);
        if (totalCredits <= 0)
            return null;

        var weightedSum = gradedCourses.Sum(c => c.FinalGrade * c.Credits);
        return weightedSum / totalCredits;
    }
}