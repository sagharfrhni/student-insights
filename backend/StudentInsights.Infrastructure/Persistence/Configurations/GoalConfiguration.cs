using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(g => g.TargetValue)
            .HasPrecision(10, 2);

        builder.Property(g => g.CurrentValue)
            .HasPrecision(10, 2);

        builder.HasOne<LearningActivity>()
                    .WithMany()
                    .HasForeignKey(g => g.RelatedActivityId)
                    .OnDelete(DeleteBehavior.SetNull);

        // GPA is a single, live-computed number (see GpaCalculator), so a
        // second active "GradePointAverage" goal for the same user would
        // just be a duplicate target for the exact same figure — restricted
        // to one at a time. Backstop for CreateGoalCommandHandler's matching
        // pre-check. Deliberately scoped to only this GoalType: StudyHours
        // and ChapterCount have no such single-source guarantee in the
        // current model (e.g. two ChapterCount goals for two different
        // textbooks are legitimately different goals), so they are left
        // unrestricted.
        builder.HasIndex(g => new { g.UserId, g.Type })
            .IsUnique()
            .HasDatabaseName("IX_Goals_UserId_Type_GpaUnique")
            .HasFilter("[IsDeleted] = 0 AND [Type] = 'GradePointAverage'");

        // A ProjectDeadline goal is keyed by which LearningActivity it
        // tracks (see Goal.Create's invariant), so two active ProjectDeadline
        // goals pointing at the same activity would be a redundant
        // duplicate. Backstop for CreateGoalCommandHandler's matching
        // pre-check.
        builder.HasIndex(g => new { g.UserId, g.RelatedActivityId })
            .IsUnique()
            .HasDatabaseName("IX_Goals_UserId_RelatedActivityId_ProjectDeadlineUnique")
            .HasFilter("[IsDeleted] = 0 AND [Type] = 'ProjectDeadline'");

        // The User relationship is configured once, in UserConfiguration
        // (Cascade). It must NOT be redefined here.
    }
}