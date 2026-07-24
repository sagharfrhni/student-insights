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
            .HasMaxLength(1000);

        builder.HasIndex(sl => new { sl.UserId, sl.StudyDateUtc });

        builder.HasOne(sl => sl.User)
            .WithMany(u => u.StudyLogs)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}