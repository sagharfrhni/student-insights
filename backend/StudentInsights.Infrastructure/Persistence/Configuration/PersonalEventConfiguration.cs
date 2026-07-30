using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence.Configurations;

public class PersonalEventConfiguration : IEntityTypeConfiguration<PersonalEvent>
{
    public void Configure(EntityTypeBuilder<PersonalEvent> builder)
    {
        builder.HasKey(pe => pe.Id);

        builder.Property(pe => pe.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pe => pe.Description)
            .HasMaxLength(2000);

        builder.HasIndex(pe => new { pe.UserId, pe.StartAtUtc });
    }
}