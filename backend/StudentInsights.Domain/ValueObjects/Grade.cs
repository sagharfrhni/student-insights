using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.ValueObjects;

/// <summary>
/// A grade on the 0–20 academic scale. Shared by Course.FinalGrade and
/// Exam.Grade so the valid range is defined once instead of being an
/// unenforced assumption duplicated in two entities.
/// </summary>
public readonly record struct Grade
{
    public const decimal MinValue = 0m;
    public const decimal MaxValue = 20m;

    public decimal Value { get; }

    public Grade(decimal value)
    {
        if (value < MinValue || value > MaxValue)
            throw new DomainException($"Grade must be between {MinValue} and {MaxValue}.");

        Value = value;
    }

    public static implicit operator decimal(Grade grade) => grade.Value;
}