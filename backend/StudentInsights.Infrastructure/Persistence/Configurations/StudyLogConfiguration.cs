using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class StudyLogConfiguration : IEntityTypeConfiguration<StudyLog>
{
    public void Configure(EntityTypeBuilder<StudyLog> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(sl => new { sl.UserId, sl.StudyDateUtc });

        // Supports the most common list query: per-course log history for
        // the current user (GetStudyLogsQuery filtered by CourseId).
        builder.HasIndex(sl => new { sl.UserId, sl.CourseId });

        builder.HasOne(sl => sl.User)
            .WithMany(u => u.StudyLogs)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}