using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(20);

        // ClassSchedule inherits AuditableEntity, not BaseEntity, so it is
        // NOT covered by the global BaseEntity RowVersion loop in
        // ApplicationDbContext.OnModelCreating. Must be configured
        // explicitly here, otherwise optimistic concurrency is silently
        // disabled for this entity.
        builder.Property(cs => cs.RowVersion)
            .IsRowVersion();

        // Note: CourseId FK / Cascade delete is configured from the Course
        // side (CourseConfiguration). Removal from Course's collection is a
        // real hard delete for this entity (see Domain review) — consistent
        // with ClassSchedule being excluded from the soft-delete query
        // filter in ApplicationDbContext.
    }
}