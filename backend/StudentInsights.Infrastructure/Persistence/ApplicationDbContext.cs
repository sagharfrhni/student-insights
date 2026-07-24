using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<LearningActivity> LearningActivities => Set<LearningActivity>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<PersonalEvent> PersonalEvents => Set<PersonalEvent>();
    public DbSet<StudyLog> StudyLogs => Set<StudyLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Soft-delete query filter: only entities that inherit
            // BaseEntity (i.e. have IsDeleted). ClassSchedule and
            // SystemSetting inherit AuditableEntity only and are
            // intentionally excluded — they are hard-deleted, not
            // soft-deleted (see Domain review).
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);

            // Optimistic concurrency for BaseEntity types. ClassSchedule and
            // SystemSetting configure RowVersion explicitly in their own
            // EntityTypeConfiguration since they fall outside this loop.
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.RowVersion))
                .IsRowVersion();
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        // Audit stamping for every IAuditable entity (BaseEntity-derived
        // entities, and SystemSetting which implements IAuditable directly).
        //
        // IAuditable's CreatedAtUtc/UpdatedAtUtc are get-only on the
        // interface, and their setters are private on AuditableEntity, by
        // deliberate Domain design — no code outside an entity should be
        // able to rewrite audit metadata directly. Infrastructure sets these
        // values through PropertyEntry.CurrentValue, which writes via EF
        // Core's own metadata-based accessor (the same mechanism EF uses to
        // materialize private-set properties from the database) rather than
        // the C# property setter, so encapsulation is preserved.
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.CreatedAtUtc)).CurrentValue = utcNow;
                    entry.Property(nameof(IAuditable.UpdatedAtUtc)).CurrentValue = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(IAuditable.UpdatedAtUtc)).CurrentValue = utcNow;
                    break;
            }
        }

        // Soft delete: intercepts a hard EntityState.Deleted (e.g. from
        // DbSet.Remove) for BaseEntity types and converts it into a soft
        // delete. Same CurrentValue approach as above, for the same
        // encapsulation reason — IsDeleted/DeletedAtUtc are otherwise only
        // ever changed via BaseEntity.Delete()/Restore().
        //
        // IMPORTANT: this only intercepts entities that are tracked and
        // explicitly marked Deleted within this SaveChanges call. It does
        // NOT, by itself, cascade a soft delete to related aggregates —
        // e.g. soft-deleting a User does not automatically soft-delete their
        // Courses, Goals, PersonalEvents, Notifications or RefreshTokens,
        // nor does soft-deleting a Course automatically soft-delete its
        // LearningActivities/Exams/StudyLogs, since those are independent
        // aggregates. The Cascade DeleteBehavior configured on the FK
        // mappings only governs actual physical DELETE statements at the
        // database level — which this interceptor prevents from happening
        // during normal application use. Cascading a soft delete across
        // aggregates is an Application-layer responsibility and must be
        // handled explicitly in the relevant command handlers (e.g.
        // DeleteUserCommandHandler, DeleteCourseCommandHandler).
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue = true;
                entry.Property(nameof(BaseEntity.DeletedAtUtc)).CurrentValue = utcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}