// StudentInsights.Application/Features/PersonalEvents/Queries/GetPersonalEvents/GetPersonalEventsQueryValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEvents;

/// <summary>
/// Field-shape validation only, run automatically by ValidationBehavior —
/// enforces "From must not be after To", mirroring GetExamsQueryValidator
/// exactly.
/// </summary>
public class GetPersonalEventsQueryValidator : AbstractValidator<GetPersonalEventsQuery>
{
    public GetPersonalEventsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");
    }
}