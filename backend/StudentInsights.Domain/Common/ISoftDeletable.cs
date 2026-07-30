namespace StudentInsights.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTime? DeletedAtUtc { get; }
}