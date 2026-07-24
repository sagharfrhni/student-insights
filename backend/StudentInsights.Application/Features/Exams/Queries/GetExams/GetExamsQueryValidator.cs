// StudentInsights.Application/Features/Exams/Queries/GetExams/GetExamsQueryValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.Exams.Queries.GetExams;

/// <summary>
/// Field-shape validation only, run automatically by ValidationBehavior —
/// enforces the "From must not be after To" edge case called out in the
/// roadmap. Course's queries have no validator because they have nothing
/// to validate; this one genuinely does, so it follows the same
/// FluentValidation + pipeline convention rather than checking it
/// manually inside the handler.
/// </summary>
public class GetExamsQueryValidator : AbstractValidator<GetExamsQuery>
{
    public GetExamsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");
    }
}