using FluentValidation;

namespace StudentInsights.Application.Features.Calendar.Queries.GetCalendarEvents;

/// <summary>
/// Field-shape validation only, run automatically by ValidationBehavior —
/// same convention as GetExamsQueryValidator. Enforces two rules from
/// Section 4 of the roadmap: FromUtc must not be after ToUtc, and the
/// range must not exceed a maximum span. The max-range check is the
/// module's only real abuse-prevention mechanism, since (unlike every
/// other list query in the project) GetCalendarEventsQuery deliberately
/// has no pagination — see GetCalendarEventsQuery's remarks.
/// </summary>
public class GetCalendarEventsQueryValidator : AbstractValidator<GetCalendarEventsQuery>
{
    private const int MaxRangeDays = 400;

    public GetCalendarEventsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => x.FromUtc <= x.ToUtc)
            .WithMessage("'FromUtc' must be earlier than or equal to 'ToUtc'.");

        RuleFor(x => x)
            .Must(x => (x.ToUtc - x.FromUtc).TotalDays <= MaxRangeDays)
            .WithMessage($"Date range must not exceed {MaxRangeDays} days.");
    }
}