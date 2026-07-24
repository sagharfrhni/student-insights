using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentInsights.Domain.ValueObjects;

namespace StudentInsights.Infrastructure.Persistence.Converters;

/// <summary>
/// Maps the Grade value object to/from the decimal column used by both
/// Course.FinalGrade and Exam.Grade. Defined once and reused so the 0–20
/// range rule (enforced inside Grade's constructor) has a single EF Core
/// mapping instead of being duplicated across two configuration files.
/// </summary>
public class GradeConverter : ValueConverter<Grade?, decimal?>
{
    public GradeConverter()
        : base(
            grade => grade == null ? null : grade.Value,
            value => value == null ? null : new Grade(value.Value))
    {
    }
}