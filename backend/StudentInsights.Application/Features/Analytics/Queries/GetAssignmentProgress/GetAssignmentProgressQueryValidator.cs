using FluentValidation;

namespace StudentInsights.Application.Features.Analytics.Queries.GetAssignmentProgress;

/// <summary>
/// Field-shape validation only, run automatically by ValidationBehavior --
/// same convention as GetExamsQueryValidator. From/To are both optional,
/// so the only real rule worth enforcing is that a supplied range isn't
/// inverted.
/// </summary>
public class GetAssignmentProgressQueryValidator : AbstractValidator<GetAssignmentProgressQuery>
{
    public GetAssignmentProgressQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");
    }
}