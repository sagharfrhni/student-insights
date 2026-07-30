namespace StudentInsights.Application.Features.Analytics.Enums;

/// <summary>
/// Time bucket for GetStudyTimeQuery. Chosen via a single query parameter
/// rather than three separate queries/endpoints, so the daily/weekly/
/// monthly bucketing logic lives once in GetStudyTimeQueryHandler instead
/// of being duplicated three times.
/// </summary>
public enum StudyTimeGranularity
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2
}