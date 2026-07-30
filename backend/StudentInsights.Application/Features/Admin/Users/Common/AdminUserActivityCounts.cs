using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Application.Features.Admin.Users.Common;

public static class AdminUserActivityCounts
{
    public static async Task<(int CourseCount, int LearningActivityCount, int ExamCount, int GoalCount)> GetAsync(
        IApplicationDbContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var courseCount = await context.Courses
            .CountAsync(c => c.UserId == userId, cancellationToken);

        var learningActivityCount = await context.LearningActivities
            .CountAsync(la => la.UserId == userId, cancellationToken);

        var examCount = await context.Exams
            .CountAsync(e => e.UserId == userId, cancellationToken);

        var goalCount = await context.Goals
            .CountAsync(g => g.UserId == userId, cancellationToken);

        return (courseCount, learningActivityCount, examCount, goalCount);
    }
}