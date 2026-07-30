using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;
using StudentInsights.Infrastructure.Persistence.Converters;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Grade)
            .HasConversion(new GradeConverter())
            .HasPrecision(4, 2);

        builder.HasIndex(e => new { e.UserId, e.ExamDateUtc });

        // Backstop for CreateExamCommandHandler/UpdateExamCommandHandler's
        // existing same-course/same-title/same-minute duplicate check. That
        // check truncates ExamDateUtc to minute precision (to ignore
        // seconds/ms noise), which a plain unique index can't express — this
        // index instead catches the narrower but more likely race: two
        // byte-identical exams (e.g. a retried/double-submitted request)
        // reaching SaveChanges concurrently before either request's
        // AnyAsync check sees the other's row.
        builder.HasIndex(e => new { e.CourseId, e.Title, e.ExamDateUtc })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(e => e.User)
            .WithMany(u => u.Exams)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}