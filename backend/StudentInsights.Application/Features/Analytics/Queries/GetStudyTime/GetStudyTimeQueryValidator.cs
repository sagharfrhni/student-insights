using FluentValidation;

namespace StudentInsights.Application.Features.Analytics.Queries.GetStudyTime;

/// <summary>
/// Field-shape validation only, run automatically by ValidationBehavior.
/// Granularity is required (unlike GetWeeklyActivityQuery's optional
/// WeekStartDate, this one has a real "missing" state worth rejecting
/// with a clear 400 instead of silently defaulting) and, if supplied,
/// must be one of the named StudyTimeGranularity values -- model binding
/// alone would otherwise accept an arbitrary out-of-range integer.
/// From/To share the same "not inverted" rule as every other Analytics
/// query.
/// </summary>
public class GetStudyTimeQueryValidator : AbstractValidator<GetStudyTimeQuery>
{
    public GetStudyTimeQueryValidator()
    {
        RuleFor(x => x.Granularity)
            .NotNull()
            .WithMessage("'Granularity' is required.")
            .IsInEnum()
            .WithMessage("'Granularity' must be Daily, Weekly, or Monthly.");

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");
    }
}