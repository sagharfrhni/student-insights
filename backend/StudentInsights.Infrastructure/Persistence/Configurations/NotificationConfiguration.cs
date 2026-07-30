using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(n => new { n.UserId, n.IsRead });

        // Supports NotificationFactory's duplicate-prevention check
        // (Where(UserId == ... && Type == ... && SourceId == ...)) used
        // by NotificationGenerationJob — not covered by the (UserId,
        // IsRead) index above, since that query never filters on IsRead.
        //
        // Deliberately NOT unique: the OverdueActivity check allows a
        // second notification for the same (UserId, Type, SourceId) once
        // an activity is completed and later reopened and becomes
        // overdue again (see the module roadmap, §17/§19), so more than
        // one row can legitimately share this triple over time. A unique
        // index only becomes correct — and only becomes necessary, since
        // this project has no multi-instance Hangfire deployment today —
        // if that reopen case is also given a way to distinguish rows
        // (e.g. a bucketing column), which is out of scope for now.
        builder.HasIndex(n => new { n.UserId, n.Type, n.SourceId });
    }
}