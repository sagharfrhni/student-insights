namespace StudentInsights.Domain.Common;

/// <summary>
/// Base for every entity that has identity and audit metadata but does not
/// necessarily need soft-delete (e.g. SystemSetting). BaseEntity builds on
/// this and adds soft-delete for the entities that need it.
/// </summary>
public abstract class AuditableEntity : IAuditable
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Used by EF Core for optimistic concurrency.
    /// Configure as IsRowVersion() in Fluent API.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Call after any state-changing operation so callers can't forget to
    /// touch the audit timestamp.
    /// </summary>
    protected void MarkModified() => UpdatedAtUtc = DateTime.UtcNow;
}