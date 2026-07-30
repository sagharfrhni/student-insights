using StudentInsights.Application.Features.Dashboard.Enums;

namespace StudentInsights.Application.Features.Dashboard.DTOs;

/// <summary>
/// A single row in the "what changed lately" feed -- a merged, capped
/// union of recently created/updated Courses, Exams, and
/// LearningActivities (see GetDashboardSummaryQueryHandler). Deliberately
/// flat and Dashboard-owned: this shape has no other legitimate home,
/// unlike UpcomingExams/GoalsProgress which reuse existing feature DTOs.
/// </summary>
public record RecentActivityDto(
    Guid Id,
    RecentActivitySourceType SourceType,
    string Title,
    DateTime OccurredAtUtc);