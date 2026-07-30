// StudentInsights.Application/Features/StudyLogs/Queries/GetStudyLogs/GetStudyLogsQueryValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogs;

/// <summary>
/// Field-shape validation only, run automatically by ValidationBehavior —
/// enforces the "From must not be after To" edge case, same rule as
/// GetExamsQueryValidator. Pagination bounds are already handled
/// defensively by PaginationParams itself, so nothing further to check
/// there.
/// </summary>
public class GetStudyLogsQueryValidator : AbstractValidator<GetStudyLogsQuery>
{
    public GetStudyLogsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");
    }
}