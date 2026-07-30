using FluentValidation;

namespace StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivityStatus;

/// <summary>
/// Deliberately shallow: the legality of a given transition (e.g. whether
/// Completed -&gt; InProgress is allowed) is a stateful/business rule, not a
/// field-shape rule, so it belongs in
/// UpdateLearningActivityStatusCommandHandler.ApplyTransition, not here —
/// same "Handler owns cross-field/stateful rules, Validator owns field
/// shape" split used for course-ownership checks throughout this module.
/// </summary>
public class UpdateLearningActivityStatusCommandValidator : AbstractValidator<UpdateLearningActivityStatusCommand>
{
    public UpdateLearningActivityStatusCommandValidator()
    {
        RuleFor(x => x.LearningActivityId)
            .NotEmpty().WithMessage("LearningActivityId is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("A valid status is required.");
    }
}