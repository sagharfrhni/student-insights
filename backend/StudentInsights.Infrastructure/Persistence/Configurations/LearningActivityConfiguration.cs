using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class LearningActivityConfiguration : IEntityTypeConfiguration<LearningActivity>
{
    public void Configure(EntityTypeBuilder<LearningActivity> builder)
    {
        builder.HasKey(la => la.Id);

        builder.Property(la => la.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(la => la.Description)
            .HasMaxLength(2000);

        builder.Property(la => la.ResourceLink)
            .HasMaxLength(500);

        builder.Property(la => la.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(la => la.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(la => la.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(la => new { la.UserId, la.DueDateUtc });

        // Backstop for CreateLearningActivityCommandHandler/
        // UpdateLearningActivityCommandHandler's same-course/same-title/
        // same-minute duplicate check — same reasoning and same minute-
        // precision mismatch as ExamConfiguration's equivalent index.
        builder.HasIndex(la => new { la.CourseId, la.Title, la.DueDateUtc })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(la => la.User)
            .WithMany(u => u.LearningActivities)
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}