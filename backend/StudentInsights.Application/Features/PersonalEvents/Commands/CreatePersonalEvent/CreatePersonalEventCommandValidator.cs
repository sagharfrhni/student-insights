// StudentInsights.Application/Features/PersonalEvents/Commands/CreatePersonalEvent/CreatePersonalEventCommandValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.CreatePersonalEvent;

/// <summary>
/// Field-shape validation only. Title/Description length limits mirror
/// PersonalEventConfiguration's actual HasMaxLength(200)/HasMaxLength(2000),
/// the same way CreateExamCommandValidator mirrors ExamConfiguration.
/// EndAtUtc &gt; StartAtUtc mirrors the exact invariant PersonalEvent.Create()
/// itself enforces (it throws DomainException when EndAtUtc &lt;= StartAtUtc)
/// — checked here too so a bad request fails fast as a 400 from
/// ValidationBehavior instead of falling through to the entity's
/// DomainException (still mapped to 400, but skips the pipeline's
/// structured per-field error response).
/// No "cannot be in the past" rule, unlike CreateExamCommandValidator —
/// the roadmap explicitly excludes that restriction for PersonalEvent,
/// since logging a past event (e.g. a meeting that already happened) is a
/// legitimate use case.
/// </summary>
public class CreatePersonalEventCommandValidator : AbstractValidator<CreatePersonalEventCommand>
{
    public CreatePersonalEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.EndAtUtc)
            .GreaterThan(x => x.StartAtUtc).WithMessage("End time must be after the start time.");

        // No .When(Description is not null) guard needed: FluentValidation's
        // MaximumLength already treats a null value as valid and only
        // checks length when a value is present.
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}