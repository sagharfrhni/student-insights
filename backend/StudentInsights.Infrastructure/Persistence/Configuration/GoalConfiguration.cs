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

        // Goal -> LearningActivity is a cross-aggregate, ID-only reference by
        // design (see Domain review): Goal exposes only RelatedActivityId,
        // with no navigation property on either side, so a Goal can never be
        // used to mutate a LearningActivity that belongs to a different
        // aggregate. Configured here with HasOne<T>()/WithMany() (no
        // navigation expressions) purely to map the FK, without
        // reintroducing the navigation properties that were deliberately
        // removed from the Domain layer.
        builder.HasOne<LearningActivity>()
            .WithMany()
            .HasForeignKey(g => g.RelatedActivityId)
            .OnDelete(DeleteBehavior.SetNull);

        // The User relationship is configured once, in UserConfiguration
        // (Cascade). It must NOT be redefined here — the previous version
        // redefined it with Restrict, which conflicts with UserConfiguration
        // and leaves the effective DeleteBehavior undefined.
    }
}