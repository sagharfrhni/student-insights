namespace StudentInsights.Domain.Common;

public abstract class BaseEntity : AuditableEntity, ISoftDeletable
{
    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        MarkModified();
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAtUtc = null;
        MarkModified();
    }
}