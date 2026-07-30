using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Stats.DTOs;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Stats.Queries.GetAdminStats;

public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var usersByActiveStatus = await _context.Users
            .GroupBy(u => u.IsActive)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeUsers = usersByActiveStatus.FirstOrDefault(x => x.Key)?.Count ?? 0;
        var inactiveUsers = usersByActiveStatus.FirstOrDefault(x => !x.Key)?.Count ?? 0;

        var usersByRole = await _context.Users
            .GroupBy(u => u.Role)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var adminCount = usersByRole.FirstOrDefault(x => x.Key == UserRole.Admin)?.Count ?? 0;
        var studentCount = usersByRole.FirstOrDefault(x => x.Key == UserRole.Student)?.Count ?? 0;

        var newUsersLast7Days = await _context.Users
            .CountAsync(u => u.CreatedAtUtc >= utcNow.AddDays(-7), cancellationToken);

        var newUsersLast30Days = await _context.Users
            .CountAsync(u => u.CreatedAtUtc >= utcNow.AddDays(-30), cancellationToken);

        var totalCourses = await _context.Courses.CountAsync(cancellationToken);

        var learningActivitiesByType = await _context.LearningActivities
            .GroupBy(la => la.Type)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalAssignments = learningActivitiesByType.FirstOrDefault(x => x.Key == ActivityType.Assignment)?.Count ?? 0;
        var totalProjects = learningActivitiesByType.FirstOrDefault(x => x.Key == ActivityType.Project)?.Count ?? 0;

        var totalExams = await _context.Exams.CountAsync(cancellationToken);

        var totalGoals = await _context.Goals.CountAsync(cancellationToken);

        var totalStudyLogs = await _context.StudyLogs.CountAsync(cancellationToken);

        var totalStudyMinutes = await _context.StudyLogs
            .SumAsync(sl => sl.DurationMinutes, cancellationToken);

        return new AdminStatsDto(
            TotalUsers: activeUsers + inactiveUsers,
            ActiveUsers: activeUsers,
            InactiveUsers: inactiveUsers,
            AdminCount: adminCount,
            StudentCount: studentCount,
            NewUsersLast7Days: newUsersLast7Days,
            NewUsersLast30Days: newUsersLast30Days,
            TotalCourses: totalCourses,
            TotalLearningActivities: totalAssignments + totalProjects,
            TotalAssignments: totalAssignments,
            TotalProjects: totalProjects,
            TotalExams: totalExams,
            TotalGoals: totalGoals,
            TotalStudyLogs: totalStudyLogs,
            TotalStudyMinutes: totalStudyMinutes);
    }
}