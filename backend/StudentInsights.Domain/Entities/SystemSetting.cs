using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.Entities;

/// <summary>
/// System-wide key-value configuration, editable at runtime without redeployment.
/// Not soft-deletable (settings are corrected or removed outright, never
/// "trashed"), so it inherits AuditableEntity directly instead of BaseEntity.
/// </summary>
public class SystemSetting : AuditableEntity
{
    private SystemSetting()
    {
    } // EF Core

    private SystemSetting(string key, string value, string? description)
    {
        Key = key;
        Value = value;
        Description = description;
    }

    public static SystemSetting Create(string key, string value, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Key is required.");
        if (value is null)
            throw new DomainException("Value is required.");

        return new SystemSetting(key.Trim(), value, description?.Trim());
    }

    /// <summary>Unique setting key (e.g. "MaxLoginAttempts").</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>Setting value stored as string; parsed by the consumer.</summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>Optional explanation of what this setting controls.</summary>
    public string? Description { get; private set; }

    public void UpdateValue(string value)
    {
        if (value is null)
            throw new DomainException("Value is required.");
        Value = value;
        MarkModified();
    }
}