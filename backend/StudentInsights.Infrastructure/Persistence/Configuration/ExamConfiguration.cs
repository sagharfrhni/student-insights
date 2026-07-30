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

        builder.HasOne(e => e.User)
            .WithMany(u => u.Exams)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}