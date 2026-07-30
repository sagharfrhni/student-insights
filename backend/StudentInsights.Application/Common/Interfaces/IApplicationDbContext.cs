using Microsoft.EntityFrameworkCore;
using StudentInsights.Domain.Entities;
using System.Collections.Generic;

namespace StudentInsights.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Course> Courses { get; }
    DbSet<ClassSchedule> ClassSchedules { get; }
    DbSet<LearningActivity> LearningActivities { get; }
    DbSet<Exam> Exams { get; }
    DbSet<Goal> Goals { get; }
    DbSet<PersonalEvent> PersonalEvents { get; }
    DbSet<StudyLog> StudyLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SystemSetting> SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}