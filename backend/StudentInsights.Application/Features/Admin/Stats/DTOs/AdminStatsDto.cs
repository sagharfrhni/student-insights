namespace StudentInsights.Application.Features.Admin.Stats.DTOs;

public record AdminStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int AdminCount,
    int StudentCount,
    int NewUsersLast7Days,
    int NewUsersLast30Days,
    int TotalCourses,
    int TotalLearningActivities,
    int TotalAssignments,
    int TotalProjects,
    int TotalExams,
    int TotalGoals,
    int TotalStudyLogs,
    int TotalStudyMinutes);