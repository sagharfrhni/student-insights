using StudentInsights.Application.Features.Exams.DTOs;
using StudentInsights.Application.Features.Goals.DTOs;

namespace StudentInsights.Application.Features.Dashboard.DTOs;

/// <summary>
/// Top-level Dashboard response. Every field is populated from an
/// independent, user-scoped read in GetDashboardSummaryQueryHandler --
/// this DTO owns no logic of its own, only shape. UpcomingExams reuses
/// ExamDto verbatim (see ExamMappingExtensions) and GoalsProgress reuses
/// GoalDto verbatim (see GoalMappingExtensions), per the roadmap's
/// "reuse an existing DTO, don't reinvent it" rule.
/// </summary>
public record DashboardSummaryDto(
    int TotalCourses,
    int ActiveAssignmentsCount,
    int ActiveProjectsCount,
    IReadOnlyList<ExamDto> UpcomingExams,
    IReadOnlyList<GoalDto> GoalsProgress,
    int WeeklyStudyMinutes,
    IReadOnlyList<RecentActivityDto> RecentActivities,
    int UnreadNotificationsCount);