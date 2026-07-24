using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;
using StudentInsights.Infrastructure.Persistence.Converters;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.InstructorName)
            .HasMaxLength(200);

        // FinalGrade is the Grade value object, not a plain decimal. EF Core
        // applies the non-nullable converter automatically to the nullable
        // Grade? property — no separate nullable converter is needed.
        builder.Property(c => c.FinalGrade)
            .HasConversion(new GradeConverter())
            .HasPrecision(4, 2);

        builder.HasMany(c => c.ClassSchedules)
            .WithOne(cs => cs.Course)
            .HasForeignKey(cs => cs.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LearningActivities)
            .WithOne(la => la.Course)
            .HasForeignKey(la => la.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Exams)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.StudyLogs)
            .WithOne(sl => sl.Course)
            .HasForeignKey(sl => sl.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}